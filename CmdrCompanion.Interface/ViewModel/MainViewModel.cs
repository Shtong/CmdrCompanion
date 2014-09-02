using CmdrCompanion.Core;
using CmdrCompanion.Interface.Modules;
using CmdrCompanion.Interface.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

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

            
        }

        private DispatcherTimer _updatePollTimer;

        private void InitializeApp()
        {
            SimpleIoc.Default.Register<EliteEnvironment>();

            // Initialize modules
            SimpleIoc.Default.Register<EmdnUpdater>(() => new EmdnUpdater(Environment));
        }

        /// <summary>
        /// Starts all the application logic that should run form the main UI thread
        /// </summary>
        public void Start()
        {
            // Configure modules
            if (Settings.Default.EmdnEnabled)
                CurrentServiceLocator.GetInstance<EmdnUpdater>().Enable();
        }

        private void InitializeUpdates()
        {
            // In 10 seconds, see if there are temp files still there
            // We wait to give some time for the updater process to exit and unlock the files
            Timer cleanTimer = new Timer(CleanupWorker, null, 10000, Timeout.Infinite);

            // Start checking for updates
            Updater.Check();

            // Every 2 seconds see if we have a result
            _updatePollTimer = new DispatcherTimer(new TimeSpan(0, 2, 0), DispatcherPriority.Background, PollUpdater, DispatcherHelper.UIDispatcher);
            _updatePollTimer.Start();
        }

        private void CleanupWorker(object state)
        {
            Updater.Cleanup();
        }

        private void PollUpdater(object sender, EventArgs e)
        {
            if(Updater.ShouldUpdate.HasValue)
            {
                _updatePollTimer.Stop();

                if(Updater.ShouldUpdate.Value)
                {
                    Updater.StartUpdating();
                    // Bye !
                    Application.Current.Shutdown();
                }
            }
        }

        public override void Cleanup()
        {
            Settings.Default.Save();

            base.Cleanup();
        }
    }
}
