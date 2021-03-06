using CmdrCompanion.Core;
using CmdrCompanion.Interface.Modules;
using CmdrCompanion.Interface.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            CloseCommand = new RelayCommand(Close);
            DoUpdateCommand = new RelayCommand(DoUpdate);
        }

        private DispatcherTimer _updatePollTimer;
        private DispatcherTimer _backupTimer;

        private void InitializeApp()
        {
            SimpleIoc.Default.Register<EliteEnvironment>();
            if(!IsInDesignMode)
                InitializePersistence();

            // Initialize modules
            SimpleIoc.Default.Register<EmdnUpdater>(() => new EmdnUpdater(Environment));
        }

        /// <summary>
        /// Starts all the application logic that should run form the main UI thread
        /// </summary>
        public void Start()
        {
            if(!IsInDesignMode)
            {
                InitializeUpdates();
            }

            // Configure modules
            if (Settings.Default.EmdnEnabled)
                CurrentServiceLocator.GetInstance<EmdnUpdater>().Enable();
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

        private void InitializeUpdates()
        {
            // In 10 seconds, see if there are temp files still there
            // We wait to give some time for the updater process to exit and unlock the files
            Timer cleanTimer = new Timer(CleanupWorker, null, 10000, Timeout.Infinite);

            // Start checking for updates
            Updater.Check();

            // Every 2 seconds see if we have a result
            _updatePollTimer = new DispatcherTimer(new TimeSpan(0, 0, 2), DispatcherPriority.Background, PollUpdater, DispatcherHelper.UIDispatcher);
        }

        public string SaveFileLocation { get; private set; }

        private void InitializePersistence()
        {
            string saveFileFolder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "CmdrCompanion");
            SaveFileLocation = Path.Combine(saveFileFolder, "save.xml");

            if (!Directory.Exists(saveFileFolder))
                Directory.CreateDirectory(saveFileFolder);

            if(File.Exists(SaveFileLocation))
            {
                try
                {
                    using(Stream s = File.OpenRead(SaveFileLocation))
                    {
                        Environment.Load(s);
                    }
                }
                catch(EnvironmentLoadException ex)
                {
                    Trace.TraceError("Could not load the saved environment ! " + ex.Message);
                }
            }

            // Do a save every 3 minutes
            _backupTimer = new DispatcherTimer(new TimeSpan(0, 3, 0), DispatcherPriority.Background, DoBackup, Dispatcher.CurrentDispatcher);
        }

        private void DoBackup(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            using (Stream s = new FileStream(SaveFileLocation, FileMode.Create))
            {
                Environment.Save(s);

                s.Flush();
            }
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
                UpdateAvailable = Updater.ShouldUpdate.Value;
            }
        }

        private bool _updateAvailable;
        public bool UpdateAvailable
        {
            get
            {
                return _updateAvailable;
            }
            set
            {
                if (value != _updateAvailable)
                {
                    _updateAvailable = value;
                    RaisePropertyChanged("UpdateAvailable");
                }
            }
        }

        public RelayCommand DoUpdateCommand { get; private set; }

        public void DoUpdate()
        {
            Updater.StartUpdating();
            // Bye !
            Application.Current.Shutdown();
        }

        public override void Cleanup()
        {
            Settings.Default.Save();
            Save();

            _backupTimer.Stop();
            _updatePollTimer.Stop();

            base.Cleanup();
        }

        public string CurrentVersion
        {
            get
            {
                return Updater.LocalVersion.ToString();
            }
        }

    }
}
