using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface
{
    public static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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
