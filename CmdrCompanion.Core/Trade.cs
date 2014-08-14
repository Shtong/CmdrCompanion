using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public class Trade : CoreObject
    {
        internal Trade(Station station, Commodity commodity)
        {
            Station = station;
            Commodity = commodity;
            DataDate = DateTime.Now;
        }

        public bool CanSell
        { 
            get
            {
                return SellingPrice > 0 && Stock > 0;
            }
        }

        public bool CanBuy 
        { 
            get
            {
                return BuyingPrice > 0;
            }
        }

        private float _sellingPrice;
        public float SellingPrice 
        { 
            get
            {
                return _sellingPrice;
            }

            set
            {
                if (value < 0)
                    value = 0;

                if (value != _sellingPrice)
                {
                    _sellingPrice = value;
                    OnPropertyChanged(new string[] { "SellingPrice", "CanSell" });
                }
            }
        }

        private float _buyingPrice;
        public float BuyingPrice 
        { 
            get
            {
                return _buyingPrice;
            }

            set
            {
                if (value < 0)
                    value = 0;

                if(value != _buyingPrice)
                {
                    _buyingPrice = value;
                    OnPropertyChanged(new string[] { "BuyingPrice", "CanBuy" });
                }
            }
        }

        private int _stock;
        public int Stock 
        { 
            get
            {
                return _stock;
            }

            set
            {
                if (value < 0)
                    value = 0;

                if(value != _stock)
                { 
                    _stock = value;
                    OnPropertyChanged(new string[] { "Stock", "CanSell" });
                }
            }
        }

        public Station Station { get; private set; }

        public Commodity Commodity { get; private set; }

        private DateTime _dataDate;
        public DateTime DataDate
        {
            get
            {
                return _dataDate;
            }

            set
            {
                if(value != _dataDate)
                {
                    _dataDate = value;
                    OnPropertyChanged("DataDate");
                }
            }
        }

        public override string ToString()
        {
            return String.Format("Trade of {0} between at station {1}", Commodity.Name, Station.Name);
        }
    }
}
