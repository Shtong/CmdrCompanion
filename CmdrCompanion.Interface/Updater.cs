using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface
{
    internal static class Updater
    {
        private const string BASE_URL = "http://cmdr-companion.shtong.mm.st/";

        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Checks on a remote server if a new version is available
        /// </summary>
        public static void Check()
        {
            if (UpdatesDisabled)
                return;

            SynchronizationContext sContext = SynchronizationContext.Current;

            if(sContext == null)
                throw new InvalidOperationException("No synchronization context is available");

            ThreadPool.QueueUserWorkItem(CheckWorker, sContext);
        }

        private static void CheckWorker(object state)
        {
            Uri baseUrl = new Uri(BASE_URL);
            SynchronizationContext parentContext = (SynchronizationContext)state;
            Version result = null;
            bool? shouldUpdate = null;
            WebClient client = new WebClient();

            // Download the latest version number

            string sVersion = null;
            try
            {
                sVersion = client.DownloadString(new Uri(baseUrl, "version.txt"));

                Version version;
                if (Version.TryParse(sVersion, out version))
                    result = version;
            }
            catch(WebException ex)
            {
                Trace.WriteLine(String.Format("[Updater] Could not download the version number: {0}", ex.Message));
            }

            if (result == null)
                shouldUpdate = false;
            else
                shouldUpdate = result != LocalVersion;

            ServerVersion = result;
            ShouldUpdate = shouldUpdate;
        }

        private static Version _serverVersion;
        public static Version ServerVersion
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _serverVersion;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            private set
            {
                _lock.EnterWriteLock();
                try
                {
                    _serverVersion = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        private static Version _localVersion;
        public static Version LocalVersion
        {
            get
            {
                if(_localVersion == null)
                    _localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return _localVersion;
            }
        }

        private static bool? _shouldUpdate;
        public static bool? ShouldUpdate
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _shouldUpdate;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            private set
            {
                _lock.EnterWriteLock();
                try
                {
                    _shouldUpdate = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Remove temporary files used by the update process
        /// </summary>
        public static void Cleanup()
        {
            string tempPath = GenerateUpdateArchivePath();
            try
            {
                if(Directory.Exists(tempPath))
                    Directory.Delete(GenerateUpdateArchivePath(), true);
            }
            catch(Exception ex)
            {
                Trace.WriteLine("[Updater] Could not remove the temp directory ({0}). We'll retry next time !", ex.Message);
            }
        }

        public static bool MainCheck(string[] args)
        {
            foreach(string arg in args)
            {
                switch(arg)
                {
                    case "--no-update":
                        UpdatesDisabled = true;
                        return true;
                }
            }

            return true;
        }

        private static bool _updatesDisabled;
        public static bool UpdatesDisabled
        {
            get { return _updatesDisabled; }
            private set
            {
                _updatesDisabled = value;
            }
        }

        private static string GenerateUpdateArchivePath()
        {
            return Path.Combine(Path.GetTempPath(), "cmdrcomp");
        }

        public static void StartUpdating()
        {
            string tempPath = GenerateUpdateArchivePath();
            string exePath = Path.Combine(tempPath, "updater.exe");
            // Extract the updater to the temp path
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            using(FileStream exe = new FileStream(exePath, FileMode.Create))
            {
                using(Stream res = Assembly.GetExecutingAssembly().GetManifestResourceStream("CmdrCompanion.Interface.ExternalDlls.updater.exe"))
                {
                    int size = 0;
                    byte[] buffer = new byte[1024];
                    do
                    {
                        size = res.Read(buffer, 0, buffer.Length);
                        if (size > 0)
                            exe.Write(buffer, 0, size);
                    } while (size > 0);
                    exe.Flush();
                }
            }

            // Launch the uploader
            ProcessStartInfo psi = new ProcessStartInfo(exePath, "\"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
            psi.Verb = "runas"; // Launch as administrator
            //psi.UseShellExecute = false;
            Process.Start(psi);
        }
    }
}
