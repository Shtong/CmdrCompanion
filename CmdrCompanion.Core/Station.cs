using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public class Station : CoreObject
    {
        internal Station(string name, Star star)
        {
            Name = name;
            Star = star;
            _trades = new ObservableCollection<Trade>();
            Trades = new ReadOnlyObservableCollection<Trade>(_trades);
            _commodityIndex = new Dictionary<Commodity, int>();
        }

        private string _name;
        public string Name 
        { 
            get
            {
                return _name;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Stations cannot have an empty name", "value");

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private Star _star;
        public Star Star 
        { 
            get
            {
                return _star;
            }
            internal set
            {
                _star = value;

                OnPropertyChanged("Star");
            }
        }

        private float _meanRadius;
        /// <summary>
        /// Gets or sets the approximative distance between the sun and the station, in Ls
        /// </summary>
        public float MeanRadius 
        { 
            get
            {
                return _meanRadius;
            }
            set
            {
                if (value < 0)
                    value = 0;

                _meanRadius = value;
                OnPropertyChanged("MeanRadius");
            }
        }

        private ObservableCollection<Trade> _trades;
        public ReadOnlyObservableCollection<Trade> Trades { get; private set; }

        private Dictionary<Commodity, int> _commodityIndex;

        public Trade CreateTrade(Commodity commodity, float sellPrice, float buyPrice, int stock)
        {
            if (commodity == null)
                throw new ArgumentNullException("commodity");

            if(_commodityIndex.ContainsKey(commodity))
                throw new ArgumentException("This commodity is already available at this station", "commodity");

            Trade result = new Trade(this, commodity)
            {
                SellingPrice = sellPrice,
                BuyingPrice = buyPrice,
                Stock = stock,
            };
            _commodityIndex.Add(commodity, Trades.Count);
            _trades.Add(result);

            return result;
        }

        public bool RemoveTrade(Commodity commodity)
        {
            if (!_commodityIndex.ContainsKey(commodity))
                return false;

            _trades.RemoveAt(_commodityIndex[commodity]);
            _commodityIndex.Remove(commodity);

            return true;
        }

        public Trade FindCommodity(Commodity commodity)
        {
            if (!_commodityIndex.ContainsKey(commodity))
                return null;
            return Trades[_commodityIndex[commodity]];
        }

        public List<TradeJumpData> FindTradesWith(Station otherStation)
        {
            if (otherStation == null)
                throw new ArgumentNullException("otherStation");

            if (otherStation == this)
                throw new ArgumentException("The otherStation argument cannot contain the Station you call the method on.", "otherStation");

            List<TradeJumpData> result = new List<TradeJumpData>();
            foreach(Trade t in Trades)
            {
                if (!t.CanSell)
                    continue;

                Trade t2 = otherStation.FindCommodity(t.Commodity);
                if (t2 == null ||!t2.CanBuy)
                    continue;

                result.Add(new TradeJumpData(t, t2));
            }

            return result;
        }

        public override string ToString()
        {
            return "Station " + Name;
        }
    }
}
