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
            _sellersList = new ObservableCollection<SellingStationViewModel>();
            _buyersList = new ObservableCollection<BuyingStationViewModel>();
            SellersView = new ListCollectionView(_sellersList);
            BuyersView = new ListCollectionView(_buyersList);
        }

        public void FillWithCommodity(Commodity data)
        {
            Commodity = data;

            _sellersList.Clear();
            _buyersList.Clear();

            foreach(Station s in Environment.Stations)
            {
                Trade t = s.FindCommodity(data);
                if(t != null && t.CanSell)
                    _sellersList.Add(new SellingStationViewModel(t, this));

                if (t != null && t.CanBuy)
                    _buyersList.Add(new BuyingStationViewModel(t, this));
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

        private ObservableCollection<SellingStationViewModel> _sellersList;
        public ListCollectionView SellersView { get; private set; }

        private ObservableCollection<BuyingStationViewModel> _buyersList;
        public ListCollectionView BuyersView { get; private set; }

        #region Sub view models
        public class SellingStationViewModel : LocalViewModelBase
        {
            public SellingStationViewModel(Trade trade, CommodityDetailsViewModel container)
            {
                Trade = trade;
            }

            public Trade Trade { get; private set; }

        }

        public class BuyingStationViewModel : LocalViewModelBase
        {
            public BuyingStationViewModel(Trade trade, CommodityDetailsViewModel container)
            {
                Trade = trade;
            }

            public Trade Trade { get; private set; }
        }
        #endregion
    }
}
