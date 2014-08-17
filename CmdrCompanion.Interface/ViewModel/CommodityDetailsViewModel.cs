using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class CommodityDetailsViewModel : LocalViewModelBase
    {
        internal CommodityDetailsViewModel()
        {
            _sellersList = new ObservableCollection<TradingStationViewModel>();
            _buyersList = new ObservableCollection<TradingStationViewModel>();
            SellersView = new ListCollectionView(_sellersList);
            BuyersView = new ListCollectionView(_buyersList);

            StationsView = new ListCollectionView(Environment.Stations);
        }

        public void FillWithCommodity(Commodity data)
        {
            ClearLists();
            Commodity = data;

            foreach(Station s in Environment.Stations)
            {
                Trade t = s.FindCommodity(data);
                if(t != null && t.CanSell)
                    _sellersList.Add(new TradingStationViewModel(t, this));

                if (t != null && t.CanBuy)
                    _buyersList.Add(new TradingStationViewModel(t, this));
            }
        }

        private Commodity _commodity;
        public Commodity Commodity 
        {
            get { return _commodity; }
            private set
            {
                if(value != _commodity)
                {
                    _commodity = value;
                    RaisePropertyChanged("Commodity");
                }
            }
        }

        private ObservableCollection<TradingStationViewModel> _sellersList;
        public ListCollectionView SellersView { get; private set; }

        private ObservableCollection<TradingStationViewModel> _buyersList;
        public ListCollectionView BuyersView { get; private set; }

        private Station _referenceStation;
        public Station ReferenceStation
        {
            get { return _referenceStation; }
            set
            {
                if(value != _referenceStation)
                {
                    _referenceStation = value;
                    RaisePropertyChanged("ReferenceStation");
                }
            }
        }

        public ListCollectionView StationsView { get; private set; }

        public override void Cleanup()
        {
            ClearLists();
            base.Cleanup();
        }

        private void ClearLists()
        {
            foreach(TradingStationViewModel vm in _sellersList)
                vm.Cleanup();
            _sellersList.Clear();
            foreach(TradingStationViewModel vm in _buyersList)
                vm.Cleanup();
            _buyersList.Clear();
        }

        #region Sub view models
        public class TradingStationViewModel : LocalViewModelBase
        {
            public TradingStationViewModel(Trade trade, CommodityDetailsViewModel container)
            {
                Trade = trade;

                _container = container;
                container.PropertyChanged += ContainerPropertyChanged;
            }

            private void ContainerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                if(e.PropertyName == "ReferenceStation")
                {
                    if (_container.ReferenceStation == null)
                        Distance = null;
                    else if (_container.ReferenceStation.Star == Trade.Station.Star)
                        Distance = 0;
                    else if (_container.ReferenceStation.Star.KnownStarProximities.ContainsKey(Trade.Station.Star))
                        Distance = _container.ReferenceStation.Star.KnownStarProximities[Trade.Station.Star];
                    else
                        Distance = null;
                }
            }

            private CommodityDetailsViewModel _container;

            public Trade Trade { get; private set; }

            private float? _distance;
            public float? Distance
            {
                get { return _distance; }
                private set
                {
                    if(value != _distance)
                    {
                        _distance = value;
                        RaisePropertyChanged("Distance");
                    }
                }
            }

            public override void Cleanup()
            {
                _container.PropertyChanged -= ContainerPropertyChanged;

                base.Cleanup();
            }
        }

        #endregion
    }
}