using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class CommodityDetailsViewModel : LocalViewModelBase
    {
        internal CommodityDetailsViewModel(Commodity data)
        {
            Commodity = data;

            _sellersList = new List<SellingStationViewModel>();
            _buyersList = new List<BuyingStationViewModel>();
            foreach(Station s in Environment.Stations)
            {
                Trade t = s.FindCommodity(data);
                if(t != null && t.CanSell)
                    _sellersList.Add(new SellingStationViewModel(t, this));

                if (t != null && t.CanBuy)
                    _buyersList.Add(new BuyingStationViewModel(t, this));
            }
            SellersView = new ListCollectionView(_sellersList);
            BuyersView = new ListCollectionView(_buyersList);
        }

        public Commodity Commodity { get; private set; }

        private List<SellingStationViewModel> _sellersList;
        public ListCollectionView SellersView { get; private set; }

        private List<BuyingStationViewModel> _buyersList;
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
