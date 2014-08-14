using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class JumpFinderViewModel : ViewModelBase
    {
        public JumpFinderViewModel()
        {
            EliteEnvironment env = ServiceLocator.Current.GetInstance<EliteEnvironment>();

            FromStationsView = new ListCollectionView(env.Stations);
            FromStationsView.Filter = FromStationFilter;
            ToStationsView = new ListCollectionView(env.Stations);
            ToStationsView.Filter = ToStationFilter;

            FromAnyCommand = new RelayCommand(FromAny);
            ToAnyCommand = new RelayCommand(ToAny);

            ToDistance = 500;
            FromDistance = 500;
        }

        private Station _fromStation;
        public Station FromStation
        {
            get { return _fromStation; }
            set
            {
                if(value != _fromStation)
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
                if(value != _fromDistance)
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

        private List<TradeJumpDataViewModel> _resultsView;
        public ListCollectionView ResultsView { get; private set; }

        private BackgroundWorker _updateResultsWorker;

        private void UpdateResults()
        {
            if(_updateResultsWorker != null)
            {
                _updateResultsWorker.CancelAsync();
            }

            foreach (TradeJumpDataViewModel data in _resultsView)
                data.Cleanup();

            _resultsView.Clear();

            _updateResultsWorker = new BackgroundWorker();
            _updateResultsWorker.DoWork += UpdateResultsWorker_DoWork;
            _updateResultsWorker.WorkerSupportsCancellation = true;
            _updateResultsWorker.RunWorkerCompleted += UpdateResultsWorker_RunWorkerCompleted;

            _updateResultsWorker.RunWorkerAsync(new Tuple<Station, Station>(FromStation, ToStation));
        }

        void UpdateResultsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _updateResultsWorker = null;
        }

        private void UpdateResultsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<Station, Station> sTuple = (Tuple<Station, Station>)e.Argument;
            Station from = sTuple.Item1;
            Station to = sTuple.Item2;
            BackgroundWorker worker = (BackgroundWorker)sender;

            if(from == null)
            {
                if(to == null)
                {
                    throw new NotImplementedException();

                }
                else
                {
                    throw new NotImplementedException();

                }
            }
            else
            {
                if(to == null)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    // EZ
                    IEnumerable<TradeJumpDataViewModel> data = from.FindTradesWith(to).Select(tjd => new TradeJumpDataViewModel(tjd));
                    DispatcherHelper.UIDispatcher.BeginInvoke(new Action<IEnumerable<TradeJumpDataViewModel>>(AddResult), data);
                }
            }
        }

        private void AddResult(TradeJumpDataViewModel result)
        {
            _resultsView.Add(result);
        }

        private void AddResult(IEnumerable<TradeJumpDataViewModel> results)
        {
            _resultsView.AddRange(results);
        }

        public class TradeJumpDataViewModel : ViewModelBase
        {
            public TradeJumpDataViewModel(TradeJumpData data)
            {

            }
        }
    }
}
