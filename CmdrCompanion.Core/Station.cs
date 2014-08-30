using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Describes a space station that the player can interact with
    /// </summary>
    /// <remarks>To create a new instance of this class, use the <see cref="Star.CreateStation"/> method.</remarks>
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
        /// <summary>
        /// Gets or sets the name of this station
        /// </summary>
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
        /// <summary>
        /// Gets the <see cref="Star"/> that this station orbits around.
        /// </summary>
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
        /// Gets or sets the approximative distance between the <see cref="Star"/> and the station, in Ls
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
        /// <summary>
        /// Gets a list of trading goods that can be exchenged at this station
        /// </summary>
        /// <seealso cref="CreateTrade"/>
        /// <seealso cref="RemoveTrade"/>
        /// <seealso cref="FindCommodity"/>
        public ReadOnlyObservableCollection<Trade> Trades { get; private set; }

        private Dictionary<Commodity, int> _commodityIndex;

        /// <summary>
        /// Creates a new <see cref="Trade"/> instance and adds it to this station
        /// </summary>
        /// <param name="commodity">The exchanged <see cref="Commodity"/></param>
        /// <param name="sellPrice">The unit price at which the station sells the commodity. Set to zero if the station does not sell.</param>
        /// <param name="buyPrice">The unit price at which the station buys the commodity. Set to zero if the station does not buy.</param>
        /// <param name="stock">The initial stock of goods, if the station is selling.</param>
        /// <returns>The newly created <see cref="Trade"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="commodity"/> parameter is null</exception>
        /// <exception cref="ArgumentException">The <paramref name="commodity"/> parameter is not know to the environment containing this station (see <see cref="EliteEnvironment.Commodities"/>)</exception>
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

        /// <summary>
        /// Removes a trade from this station
        /// </summary>
        /// <param name="commodity">The <see cref="Commodity"/> that should be removed from the trading list</param>
        /// <returns>true is the commodity was removed, false if it was not found in this station's trades.</returns>
        public bool RemoveTrade(Commodity commodity)
        {
            if (!_commodityIndex.ContainsKey(commodity))
                return false;

            _trades.RemoveAt(_commodityIndex[commodity]);
            _commodityIndex.Remove(commodity);

            return true;
        }

        /// <summary>
        /// Tries to find a <see cref="Trade"/> instance in this station's trades that matches the specified <see cref="Commodity"/>.
        /// </summary>
        /// <param name="commodity">The <see cref="Commodity"/> to look for</param>
        /// <returns>A <see cref="Trade"/> instance, or null if no trade was found.</returns>
        public Trade FindCommodity(Commodity commodity)
        {
            if (!_commodityIndex.ContainsKey(commodity))
                return null;
            return Trades[_commodityIndex[commodity]];
        }

        /// <summary>
        /// Returns a list of possible commodities that can be bought in this station and sold in another specified station.
        /// </summary>
        /// <param name="otherStation">A station to sell at</param>
        /// <returns>A list of <see cref="TradeJumpData"/> instances, describing the available trading options</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="otherStation"/> parameter is null</exception>
        /// <exception cref="ArgumentException">The <paramref name="otherStation"/> parameter equals the current station</exception>
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
