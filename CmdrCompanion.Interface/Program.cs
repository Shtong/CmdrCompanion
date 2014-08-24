using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface
{
    public static class Program
    {
        private static EventWaitHandle _appSingleton;

        [STAThread]
        private static void Main(string[] args)
        {
            // This handle is actually used by the updater, that has to wait for the application to exit before updating it
            _appSingleton = new EventWaitHandle(false, EventResetMode.ManualReset, "CmdrCompanion#running");

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // check for updating tasks
            if (!Updater.MainCheck(args))
                return;

            App app = new App();
            app.InitializeComponent();
            app.Run();
        }

        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string asmName = String.Format("CmdrCompanion.Interface.ExternalDlls.{0}.dll", new AssemblyName(args.Name).Name);
            using(Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(asmName))
            {
                byte[] asmBytes = new byte[s.Length];
                s.Read(asmBytes, 0, asmBytes.Length);
                return Assembly.Load(asmBytes);
            }
        }
    }
}
