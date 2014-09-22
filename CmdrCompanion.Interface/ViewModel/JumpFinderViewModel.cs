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
            FromStationSelector = new StationSelectorViewModel()
            {
                UserCanSelectAny = true,
                UserCanSelectCurrent = true,
            };
            FromStationSelector.PropertyChanged += FromStationSelector_PropertyChanged;
            ToStationSelector = new StationSelectorViewModel()
            {
                UserCanSelectAny = true,
                UserCanSelectCurrent = true,
            };
            ToStationSelector.PropertyChanged += ToStationSelector_PropertyChanged;

            FromStationSelector.Filter = FromStationFilter;
            ToStationSelector.Filter = ToStationFilter;

            _resultsList = new ObservableCollection<TradeJumpDataViewModel>();
            ResultsView = new ListCollectionView(_resultsList);
            ResultsView.Filter = ResultsViewFilter;

            StartUpdatingCommand = new RelayCommand(StartUpdating);

            MaxDistance = 500;
            CargoReference = 4;
            MaximumPrice = 1000;
            LowerProfitsThreshold = 1;
        }

        private void ToStationSelector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "SelectedStation":
                    FromStationSelector.Refresh();
                    RaisePropertyChanged("MaxDistanceEnabled");
                    break;
            }
        }

        private void FromStationSelector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedStation":
                    ToStationSelector.Refresh();
                    RaisePropertyChanged("MaxDistanceEnabled");
                    break;
            }
        }

        private float _maxDistance;
        public float MaxDistance
        {
            get { return _maxDistance; }
            set
            {
                if (value != _maxDistance)
                {
                    _maxDistance = value;
                    RaisePropertyChanged("MaxDistance");
                }
            }
        }

        public bool MaxDistanceEnabled
        {
            get
            {
                return ToStationSelector.SelectedStation == null || FromStationSelector.SelectedStation == null;
            }
        }

        private bool FromStationFilter(object fromStation)
        {
            if (ToStationSelector.SelectedStation == null)
                return true;

            return fromStation != ToStationSelector.SelectedStation;
        }

        private bool ToStationFilter(object toStation)
        {
            if (FromStationSelector.SelectedStation == null)
                return true;

            return toStation != FromStationSelector.SelectedStation;
        }

        public StationSelectorViewModel FromStationSelector { get; private set; }
        public StationSelectorViewModel ToStationSelector { get; private set; }

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

            AstronomicalObject[] sList = null;
            if (FromStationSelector.SelectedStation == null || ToStationSelector.SelectedStation == null)
                sList = Environment.Stations.ToArray();

            IsWorking = true;
            _updateResultsWorker.RunWorkerAsync(new UpdateResultsData()
            {
                from = FromStationSelector.SelectedStation,
                to = ToStationSelector.SelectedStation,
                maxDistance = MaxDistance,
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
                    foreach (AstronomicalObject s1 in data.stationList)
                    {
                        foreach (AstronomicalObject s2 in data.stationList)
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
                    foreach (AstronomicalObject s in data.stationList)
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
                    foreach (AstronomicalObject s in data.stationList)
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
            public AstronomicalObject from;
            public AstronomicalObject to;
            public float maxDistance;
            public AstronomicalObject[] stationList;
        }
    }
}
