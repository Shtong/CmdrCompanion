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
            SimpleIoc.Default.Register<MarketDump>();
        }

        /// <summary>
        /// Starts all the application logic that should run form the main UI thread
        /// </summary>
        public void Start()
        {
            if(!IsInDesignMode)
            {
                InitializeUpdates();
                InitializeMarketdump();
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

        private void InitializeMarketdump()
        {
            /*
            if(!Settings.Default.EmdnContribInviteShown)
            {
                MyMessageBoxViewModel messageBox = new MyMessageBoxViewModel();
                messageBox.Title = "EMDN contribution";
                messageBox.MainText = "This software gets the trade data online from EMDN, the Elite Market Data Network. Would you please consider launching a background tool that will read your game's data and contribute back to the other users?\n\nThis requires administrator privileges.";
                messageBox.WindowClosed += () =>
                {
                    if(messageBox.Result == MessageBoxResult.OK)
                    {
                        Settings.Default.UseMartketdumpWithEmdn = true;
                    }

                    Settings.Default.EmdnContribInviteShown = true;
                    Settings.Default.Save();
                };
                MessengerInstance.Send(new ShowMessageBoxMessage(messageBox));
            }
            */

            MarketDump wrapper = CurrentServiceLocator.GetInstance<MarketDump>();
            wrapper.MarketDataReceived += wrapper_MarketDataReceived;
            wrapper.NewLocation += wrapper_NewLocation;

            wrapper.Start();
        }

        void wrapper_NewLocation(object sender, NewLocationEventArgs e)
        {
            Trace.TraceInformation("Received location data: {0} -> {1} ({2})", e.StarName, e.PositionDescription, e.IsInDeepSpace);

            Star s = Environment.FindObjectByName<Star>(e.StarName);
            if(s == null)
                s = Environment.CreateStar(e.StarName);

            AstronomicalObject ao = null;
            if(e.IsInDeepSpace)
                ao = Environment.GetDeepSpaceObject(s);
            else
            {
                ao = Environment.FindObjectByName(e.PositionDescription);
                if (ao == null)
                    ao = s.CreateAstronomicalObject(e.PositionDescription);
            }

            Environment.CurrentSituation.CurrentLocation = ao;
        }

        private void wrapper_MarketDataReceived(object sender, MarketDataReceivedEventArgs e)
        {
            // Check that the star exists
            Star s = Environment.FindObjectByName<Star>(e.StarName);
            if (s == null)
                s = Environment.CreateStar(e.StarName);

            // Check that the station exists
            Station station = s.FindObjectByName<Station>(e.StationName);
            if (station == null)
                station = s.CreateStation(e.StationName);

            // Check that this commodity exists
            Commodity com = Environment.FindCommodityByName(e.ItemName);
            if(com == null)
                Environment.CreateCommodity(e.ItemName, e.CategoryName);

            // Check that the commodity isn't already available
            Trade t = station.FindCommodity(com);
            if(t == null)
            {
                t = station.CreateTrade(com, e.SellPrice, e.BuyPrice, e.StationStock);
            }
            else
            {
                t.BuyingPrice = e.BuyPrice;
                t.SellingPrice = e.SellPrice;
                t.Stock = e.StationStock;
            }
            t.DataDate = DateTime.Now;
            t.DataSourceName = "Local";
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
            CurrentServiceLocator.GetInstance<MarketDump>().Dispose();

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
