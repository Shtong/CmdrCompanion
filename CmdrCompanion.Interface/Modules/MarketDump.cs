using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CmdrCompanion.Interface.Modules
{
    /// <summary>
    /// A wrapper around the marketdump tool
    /// </summary>
    public sealed class MarketDump : IDisposable
    {
        private const string RESOURCE_NAME = "CmdrCompanion.Interface.Resources.marketdump.zip";
        private const string CURRENT_TOOL_VERSION = "0.5.4";
        private const string DEEP_SPACE_NAME = "LOCATION_deep_space;";

        ~MarketDump()
        {
            Dispose(false);
        }

        public bool Running
        {
            get;
            private set;
        }

        private CancellationTokenSource _cancelSource = null;

        private static volatile bool _extracted;
        // The extraction is locked over all instances as all instances will use the same files
        private static object _extractLock = new Object();

        public bool Disposed { get; private set; }

        // These 2 fields are only accessed from the worker thread
        private BackgroundStreamWrapper _stdoutWrapper;
        private BackgroundStreamWrapper _stderrWrapper;

        // Warning : launched from a worker thread
        public void Start(bool showWindow = false)
        {
            if (Disposed)
                throw new ObjectDisposedException("MarketDump Wrapper");

            if (Running)
                return;

            Running = true;

            _cancelSource = new CancellationTokenSource();

            ThreadPool.QueueUserWorkItem(StartWorker, new StartWorkerAgs()
            {
                showWindow = showWindow,
                cancellationToken = _cancelSource.Token,
                uiDispatcher = Dispatcher.CurrentDispatcher,
            });
        }

        public void Stop()
        {
            if(Disposed)
                throw new ObjectDisposedException("MarketDump Wrapper");

            if (!Running)
                return;

            _cancelSource.Cancel();
            _cancelSource = null;
            Running = false;
        }

        #region Events management
        public event EventHandler<NewLocationEventArgs> NewLocation;

        private void OnNewLocation(NewLocationEventArgs args)
        {
            if (NewLocation != null)
                NewLocation(this, args);
        }

        public event EventHandler<MarketDataReceivedEventArgs> MarketDataReceived;

        private void OnMarketDataReceived(MarketDataReceivedEventArgs args)
        {
            if (MarketDataReceived != null)
                MarketDataReceived(this, args);
        }
        #endregion

        #region Private methods and tools
        private static void Extract()
        {
            if (!_extracted)
            {
                lock (_extractLock)
                {
                    if (!_extracted)
                    {
                        // Was the tool installed in a previous run ?
                        string dirName = GetToolFolderName();
                        if (!Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                            string archiveName = Path.Combine(dirName, "a.zip");

                            // Copy the archive
                            using (Stream source = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME))
                            {
                                using (Stream dest = File.Create(archiveName))
                                {
                                    source.CopyTo(dest);
                                }
                            }

                            // Extract the archive
                            ZipFile.ExtractToDirectory(archiveName, dirName);
                        }
                        _extracted = true;
                    }
                }
            }
        }

        private static string GetToolFolderName()
        {
            return Path.Combine(Path.GetTempPath(), "cc_marketdump_" + CURRENT_TOOL_VERSION);
        }

        private void StartWorker(object state)
        {
            StartWorkerAgs args = (StartWorkerAgs)state;

            // Prepare the files
            Extract();

            // And launch
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(GetToolFolderName(), "marketdump.exe"));
            //psi.Verb = "runas"; // So it seems that the marketdump tool does not need admin rights
            psi.UseShellExecute = false;
            psi.WindowStyle = args.showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = !args.showWindow;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardInput = true;

            if(!args.cancellationToken.IsCancellationRequested)
            {
                using(Process p = Process.Start(psi))
                {
                    _stdoutWrapper = new BackgroundStreamWrapper(p.StandardOutput);
                    _stderrWrapper = new BackgroundStreamWrapper(p.StandardError);

                    // Wait for exit request
                    // while checking if process wrote some relevant stuff in
                    // its outputs
                    while(!args.cancellationToken.WaitHandle.WaitOne(500))
                    {
                        ProcessOutputs(args.uiDispatcher);
                    }

                    _stdoutWrapper = null;
                    _stderrWrapper = null;
                    p.Kill(); // A little bit violent maybe, but there does not seem to be a better way through the process class
                              // Maybe by sending some keys to the input ?
                }
            }
        }

        private void ProcessOutputs(Dispatcher uiDispatcher)
        {
            string msg;
            while ((msg = _stdoutWrapper.GetNextMessage()) != null)
            {
                Trace.TraceInformation("-- " + msg);

                if(!msg.StartsWith("buyPrice"))
                {
                    // If it's not the header then it's an entry
                    MarketDataReceivedEventArgs args = null;
                    try
                    {
                        string[] parts = msg.Split(',');
                        Tuple<string, string> locations = ParseLocationName(parts[8]);

                        args = new MarketDataReceivedEventArgs(
                            Single.Parse(parts[0]),
                            Single.Parse(parts[1]),
                            Int32.Parse(parts[2]),
                            Int32.Parse(parts[3]),
                            Int32.Parse(parts[4]),
                            Int32.Parse(parts[5]),
                            parts[6],
                            parts[7],
                            locations.Item2,
                            locations.Item1,
                            new DateTime(1970, 1, 1).AddSeconds(Int32.Parse(parts[9])));
                    }
                    catch(Exception ex)
                    {
                        Trace.TraceWarning("[MarketDump] An error occured while parsing a marketdump market entry: " + ex);
                        Trace.TraceWarning("[MarketDump] The entry contained: " + msg);
                    }

                    if(args != null)
                    {
                        uiDispatcher.BeginInvoke(new Action<MarketDataReceivedEventArgs>(OnMarketDataReceived), DispatcherPriority.Background, args);
                    }
                }
            }


            while ((msg = _stderrWrapper.GetNextMessage()) != null)
            {
                // Remove the "[*]  " part
                msg = msg.Substring(5);
                Trace.TraceInformation("## " + msg);

                if(msg.StartsWith("Location: "))
                {
                    // New location detected !
                    Tuple<string, string> names = null;
                    
                    try
                    {
                        names = ParseLocationName(msg.Substring(10));
                    }
                    catch(Exception ex)
                    {
                        Trace.TraceWarning("[MarketDump] An error occured while parsing a marketdump location entry: " + ex);
                        Trace.TraceWarning("[MarketDump] The entry contained: " + msg);
                    }

                    if(names != null)
                    {
                        NewLocationEventArgs args = new NewLocationEventArgs(
                            names.Item1,
                            names.Item2,
                            names.Item2 == DEEP_SPACE_NAME);

                        uiDispatcher.BeginInvoke(new Action<NewLocationEventArgs>(OnNewLocation), args);
                    }
                }
            }
        }

        private sealed class StartWorkerAgs
        {
            public bool showWindow;
            public CancellationToken cancellationToken;
            public Dispatcher uiDispatcher;
        }

        private static Tuple<string, string> ParseLocationName(string fullLocationName)
        {
            string[] parts = fullLocationName.Split('(');
            return new Tuple<string, string>(
                parts[0].Substring(0, parts[0].Length - 1),
                parts[1].Substring(0, parts[1].Length - 1));
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing)
            {
                Stop();
            }

            Disposed = true;
            // TODO : Find a way to kill the marketdump process during finalization
            // so it doesn't stay up in case of crash
        }
        #endregion

    }
}
