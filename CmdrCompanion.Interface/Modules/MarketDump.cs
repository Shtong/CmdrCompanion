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

namespace CmdrCompanion.Interface.Modules
{
    /// <summary>
    /// A wrapper around the marketdump tool
    /// </summary>
    public sealed class MarketDump : IDisposable
    {
        private const string RESOURCE_NAME = "CmdrCompanion.Interface.Resources.marketdump.zip";
        private const string CURRENT_TOOL_VERSION = "0.5.3";

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

        // Warning : launched from a worker thread
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
            });
        }

        private void StartWorker(object state)
        {
            StartWorkerAgs args = (StartWorkerAgs)state;

            // Prepare the files
            Extract();

            // And launch
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(GetToolFolderName(), "marketdump.exe"));
            psi.Verb = "runas";
            psi.WindowStyle = args.showWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = !args.showWindow;


            if(!args.cancellationToken.IsCancellationRequested)
            {
                using(Process p = Process.Start(psi))
                {
                    // Wait for exit request
                    args.cancellationToken.WaitHandle.WaitOne();

                    p.Kill(); // A little bit violent maybe, but there does not seem to be a better way through the process class
                              // Maybe by sending some keys to the input ?
                }
            }
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

        private sealed class StartWorkerAgs
        {
            public bool showWindow;
            public CancellationToken cancellationToken;
        }

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

    }
}
