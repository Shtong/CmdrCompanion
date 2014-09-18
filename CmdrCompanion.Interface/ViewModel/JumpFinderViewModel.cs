using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class JumpFinderViewModel : LocalViewModelBase
    {
        public JumpFinderViewModel()
        {
            FromStationsView = new ListCollectionView(Environment.Stations);
            FromStationsView.SortDescriptions.Add(new SortDescription("Star.Name", ListSortDirection.Ascending));
            FromStationsView.Filter = FromStationFilter;
            ToStationsView = new ListCollectionView(Environment.Stations);
            ToStationsView.SortDescriptions.Add(new SortDescription("Star.Name", ListSortDirection.Ascending));
            ToStationsView.Filter = ToStationFilter;

            _resultsList = new ObservableCollection<TradeJumpDataViewModel>();
            ResultsView = new ListCollectionView(_resultsList);
            ResultsView.Filter = ResultsViewFilter;

            FromAnyCommand = new RelayCommand(FromAny);
            ToAnyCommand = new RelayCommand(ToAny);
            StartUpdatingCommand = new RelayCommand(StartUpdating);

            ToDistance = 500;
            FromDistance = 500;
            CargoReference = 4;
            MaximumPrice = 1000;
            LowerProfitsThreshold = 1;
        }

        private Station _fromStation;
        public Station FromStation
        {
            get { return _fromStation; }
            set
            {
                if (value != _fromStation)
                {
                    _fromStation = value;
                    RaisePropertyChanged("FromStation");
                    RaisePropertyChanged("FromDistanceEnabled");
                    RaisePropertyChanged("FromAnyEnabled");
                    ToStationsView.Refresh();
                }
            }
        }

        private float _fromDistance;
        public float FromDistance
        {
            get { return _fromDistance; }
            set
            {
                if (value != _fromDistance)
                {
                    _fromDistance = value;
                    RaisePropertyChanged("FromDistance");
                }
            }
        }

        public bool FromDistanceEnabled
        {
            get
            {
                return FromStation == null && ToStation != null;
            }
        }

        public bool FromAnyEnabled
        {
            get
            {
                return FromStation != null;
            }
        }

        private Station _toStation;
        public Station ToStation
        {
            get { return _toStation; }
            set
            {
                if (value != _toStation)
                {
                    _toStation = value;
                    RaisePropertyChanged("ToStation");
                    RaisePropertyChanged("FromDistanceEnabled");
                    RaisePropertyChanged("ToDistanceEnabled");
                    RaisePropertyChanged("ToAnyEnabled");
                    FromStationsView.Refresh();
                }
            }
        }

        private float _toDistance;
        public float ToDistance
        {
            get { return _toDistance; }
            set
            {
                if (value != _toDistance)
                {
                    _toDistance = value;
                    RaisePropertyChanged("ToDistance");
                }
            }
        }

        public bool ToDistanceEnabled
        {
            get
            {
                return ToStation == null;
            }
        }

        public bool ToAnyEnabled
        {
            get
            {
                return ToStation != null;
            }
        }

        public ListCollectionView FromStationsView { get; private set; }
        private bool FromStationFilter(object fromStation)
        {
            if (ToStation == null)
                return true;

            return fromStation != ToStation;
        }
        public ListCollectionView ToStationsView { get; private set; }
        private bool ToStationFilter(object toStation)
        {
            if (FromStation == null)
                return true;

            return toStation != FromStation;
        }

        private bool _isWorking;
        public bool IsWorking
        {
            get
            {
                return _isWorking;
            }

            set
            {
                if(value != _isWorking)
                {
                    _isWorking = value;
                    RaisePropertyChanged("IsWorking");
                }
            }
        }

        private bool _showCargoIndicators;
        public bool ShowCargoIndicators
        {
            get { return _showCargoIndicators; }
            set
            {
                if(value != _showCargoIndicators)
                {
                    _showCargoIndicators = value;
                    RaisePropertyChanged("ShowCargoIndicators");
                    RaisePropertyChanged("CanFilterMaximumPrice");
                }
            }
        }

        private int _cargoReference;
        public int CargoReference
        {
            get { return _cargoReference; }
            set
            {
                if(value != _cargoReference)
                {
                    _cargoReference = value;
                    RaisePropertyChanged("CargoReference");
                }
            }
        }

        private bool _filterLowerProfits;
        public bool FilterLowerProfits
        {
            get
            {
                return _filterLowerProfits;
            }

            set
            {
                if(value != _filterLowerProfits)
                {
                    _filterLowerProfits = value;
                    RaisePropertyChanged("FilterLowerProfits");
                    ResultsView.Refresh();
                }
            }
        }

        private float _lowerProfitsThreshold;
        public float LowerProfitsThreshold
        {
            get { return _lowerProfitsThreshold; }
            set
            {
                if(value != _lowerProfitsThreshold)
                {
                    _lowerProfitsThreshold = value;
                    RaisePropertyChanged("LowerProfitsThreshold");

                    if (FilterLowerProfits)
                        ResultsView.Refresh();
                }
            }
        }

        private bool _filterMaximumPrice;
        public bool FilterMaximumPrice
        {
            get { return _filterMaximumPrice; }
            set
            {
                if(value != _filterMaximumPrice)
                {
                    _filterMaximumPrice = value;
                    RaisePropertyChanged("FilterMaximumPrice");

                    ResultsView.Refresh();
                }
            }
        }

        private float _maximumPrice;
        public float MaximumPrice
        {
            get { return _maximumPrice; }
            set
            {
                if(value != _maximumPrice)
                {
                    _maximumPrice = value;
                    RaisePropertyChanged("MaximumPrice");

                    if (FilterMaximumPrice)
                        ResultsView.Refresh();
                }
            }
        }

        public bool CanFilterMaximumPrice
        {
            get
            {
                return ShowCargoIndicators;
            }
        }

        public RelayCommand ToAnyCommand { get; private set; }
        public RelayCommand FromAnyCommand { get; private set; }

        public void ToAny()
        {
            ToStationsView.MoveCurrentToPosition(-1);
        }

        public void FromAny()
        {
            FromStationsView.MoveCurrentToPosition(-1);
        }

        public RelayCommand StartUpdatingCommand { get; private set; }

        public void StartUpdating()
        {
            UpdateResults();
        }

        private ObservableCollection<TradeJumpDataViewModel> _resultsList;
        public ListCollectionView ResultsView { get; private set; }

        private bool ResultsViewFilter(object data)
        {
            if(data is TradeJumpDataViewModel)
            {
                TradeJumpDataViewModel vmData = (TradeJumpDataViewModel)data;
                return !(FilterLowerProfits && vmData.RawData.ProfitPerUnit < LowerProfitsThreshold) && !(FilterMaximumPrice && vmData.TotalCost > MaximumPrice);
            }

            return false;
        }

        private BackgroundWorker _updateResultsWorker;

        private void UpdateResults()
        {
            if (_updateResultsWorker != null)
            {
                _updateResultsWorker.CancelAsync();
            }

            foreach (TradeJumpDataViewModel data in _resultsList)
                data.Cleanup();

            _resultsList.Clear();

            _updateResultsWorker = new BackgroundWorker();
            _updateResultsWorker.DoWork += UpdateResultsWorker_DoWork;
            _updateResultsWorker.WorkerSupportsCancellation = true;
            _updateResultsWorker.RunWorkerCompleted += UpdateResultsWorker_RunWorkerCompleted;

            Station[] sList = null;
            if (FromStation == null || ToStation == null)
                sList = Environment.Stations.ToArray();

            IsWorking = true;
            _updateResultsWorker.RunWorkerAsync(new UpdateResultsData()
            {
                from = FromStation,
                to = ToStation,
                maxDistance = ToStation == null ? ToDistance : FromDistance,
                stationList = sList,
            });
        }

        void UpdateResultsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _updateResultsWorker = null;
            IsWorking = false;
        }

        private void UpdateResultsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateResultsData data = (UpdateResultsData)e.Argument;
            BackgroundWorker worker = (BackgroundWorker)sender;

            if (data.from == null)
            {
                if (data.to == null)
                {
                    foreach (Station s1 in data.stationList)
                    {
                        foreach (Station s2 in data.stationList)
                        {
                            if (worker.CancellationPending)
                                return;

                            if (s1 == s2)
                                continue;

                            if (!s1.Star.KnownStarProximities.ContainsKey(s2.Star) || s1.Star.KnownStarProximities[s2.Star] > data.maxDistance)
                                continue;

                            IEnumerable<TradeJumpDataViewModel> results = s1.FindTradesWith(s2).Select(tjd => new TradeJumpDataViewModel(tjd, this));
                            DispatcherHelper.UIDispatcher.BeginInvoke(new Action<IEnumerable<TradeJumpDataViewModel>>(AddResult), results);
                        }
                    }

                }
                else
                {
                    foreach (Station s in data.stationList)
                    {
                        if (worker.CancellationPending)
                            return;

                        if (s == data.to)
                            continue;

                        if (!s.Star.KnownStarProximities.ContainsKey(data.to.Star) || s.Star.KnownStarProximities[data.to.Star] > data.maxDistance)
                            continue;

                        IEnumerable<TradeJumpDataViewModel> results = s.FindTradesWith(data.to).Select(tjd => new TradeJumpDataViewModel(tjd, this));
                        DispatcherHelper.UIDispatcher.BeginInvoke(new Action<IEnumerable<TradeJumpDataViewModel>>(AddResult), results);
                    }
                }
            }
            else
            {
                if (data.to == null)
                {
                    foreach (Station s in data.stationList)
                    {
                        if (worker.CancellationPending)
                            return;

                        if (s == data.from)
                            continue;

                        if (!data.from.Star.KnownStarProximities.ContainsKey(s.Star) || data.from.Star.KnownStarProximities[s.Star] > data.maxDistance)
                            continue;

                        IEnumerable<TradeJumpDataViewModel> results = data.from.FindTradesWith(s).Select(tjd => new TradeJumpDataViewModel(tjd, this));
                        DispatcherHelper.UIDispatcher.BeginInvoke(new Action<IEnumerable<TradeJumpDataViewModel>>(AddResult), results);
                    }
                }
                else
                {
                    // EZ
                    IEnumerable<TradeJumpDataViewModel> results = data.from.FindTradesWith(data.to).Select(tjd => new TradeJumpDataViewModel(tjd, this));
                    DispatcherHelper.UIDispatcher.BeginInvoke(new Action<IEnumerable<TradeJumpDataViewModel>>(AddResult), results);
                }
            }
        }

        private void AddResult(IEnumerable<TradeJumpDataViewModel> results)
        {
            foreach (TradeJumpDataViewModel data in results)
                _resultsList.Add(data);
        }

        public class TradeJumpDataViewModel : ViewModelBase
        {
            internal TradeJumpDataViewModel(TradeJumpData data, JumpFinderViewModel container)
            {
                RawData = data;

                if (data.From.Station.Star.KnownStarProximities.ContainsKey(data.To.Station.Star))
                    Distance = data.From.Station.Star.KnownStarProximities[data.To.Station.Star];

                container.PropertyChanged += ContainerPropertyChanged;
                _container = container;
            }

            void ContainerPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (!_container.ShowCargoIndicators)
                    return;

                if(e.PropertyName == "CargoReference")
                {
                    RaisePropertyChanged("TotalCost");
                    RaisePropertyChanged("TotalProfit");
                    RaisePropertyChanged("TotalRevenue");
                }
            }

            private JumpFinderViewModel _container;

            public TradeJumpData RawData { get; private set; }

            public float Distance { get; private set; }

            public float TotalCost
            {
                get
                {
                    return _container.CargoReference * RawData.From.SellingPrice;
                }
            }

            public float TotalProfit { 
                get 
                { 
                    return _container.CargoReference * RawData.ProfitPerUnit;
                } 
            }

            public float TotalRevenue { 
                get
                {
                    return _container.CargoReference * RawData.To.BuyingPrice; 
                }
            }

            public override void Cleanup()
            {
                _container.PropertyChanged -= ContainerPropertyChanged;

                base.Cleanup();
            }
        }

        private sealed class UpdateResultsData
        {
            public Station from;
            public Station to;
            public float maxDistance;
            public Station[] stationList;
        }
    }
}
