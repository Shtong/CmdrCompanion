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

        /// <summary>
        /// Gets a value indicating whether the station has this commodity available for sale
        /// </summary>
        public bool CanSell
        { 
            get
            {
                return SellingPrice > 0 && Stock > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the station can buy this commodity
        /// </summary>
        public bool CanBuy 
        { 
            get
            {
                return BuyingPrice > 0;
            }
        }

        private float _sellingPrice;
        /// <summary>
        /// Gets of sets the price at which the station will sell this commodity to traders
        /// </summary>
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
        /// <summary>
        /// Gets of sets the price at which the station will buy this commodity from traders
        /// </summary>
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
        /// <summary>
        /// Gets of sets the amount of units available for sale
        /// </summary>
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
        /// <summary>
        /// Gets or sets the date at which the data in this instance was extracted from the game
        /// </summary>
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
