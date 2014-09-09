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
    public sealed class MarketDump
    {
        private const string RESOURCE_NAME = "CmdrCompanion.Interface.Resources.marketdump.zip";
        private const string CURRENT_TOOL_VERSION = "0.5.3";

        public bool Running
        {
            get;
            private set;
        }

        private CancellationTokenSource _cancelSource = null;

        private volatile bool _extracted;
        // The extraction is locked over all instances as all instances will use the same files
        private static object _extractLock = new Object();

        // Warning : launched from a worker thread
        private void Extract()
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
            psi.UseShellExecute = true;
            psi.Verb = "runat";
            psi.CreateNoWindow = !args.showWindow;


            if(!args.cancellationToken.IsCancellationRequested)
            {
                using(Process p = Process.Start(psi))
                {
                    // Wait for exit request
                    args.cancellationToken.WaitHandle.WaitOne();

                    p.CloseMainWindow();
                }
            }
        }

        public void Stop()
        {
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
    }
}
