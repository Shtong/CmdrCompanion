using CmdrCompanion.Core;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CmdrCompanion.Interface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;

            StringBuilder builder = new StringBuilder();
            builder.Append("---------------");
            builder.AppendLine(DateTime.Now.ToString());

            while(ex != null)
            {
                builder.AppendLine("vvvvv");
                builder.Append(ex.GetType().Name).Append(": ");
                builder.AppendLine(ex.Message);
                builder.AppendLine(ex.StackTrace);

                ex = ex.InnerException;
            }

            File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "crash.txt"), builder.ToString());
        }
    }
}
