using CmdrCompanion.Core;
using CmdrCompanion.Interface.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CmdrCompanion.Interface.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : LocalViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            InitializeApp();

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            CloseCommand = new RelayCommand(Close);
        }


        private void InitializeApp()
        {
            SimpleIoc.Default.Register<EliteEnvironment>();

            SimpleIoc.Default.Register<PluginManager>(() =>
            {
                PluginManager pm = new PluginManager();
                pm.ScanForPlugins();
                return pm;
            });
        }

        /// <summary>
        /// Starts all the application logic that should run form the main UI thread
        /// </summary>
        public void Start()
        {
            PluginManager pm = CurrentServiceLocator.GetInstance<PluginManager>();
            // Auto-start previously started plugins
            if (Settings.Default.ActivatedPlugins != null)
            {
                foreach (string name in Settings.Default.ActivatedPlugins)
                {
                    PluginContainer container = pm.AvailablePlugins.FirstOrDefault(pc => pc.Name == name);
                    if (container != null)
                    {
                        container.Initialize();
                        container.Activate();
                    }
                }
            }
        }

        public RelayCommand CloseCommand { get; private set; }

        public void Close()
        {
            MessengerInstance.Send(new CloseMessage());
        }

        public class CloseMessage : GenericMessage<object>
        {
            public CloseMessage() : base(null)
            {

            }
        }
    }
}
