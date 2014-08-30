using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Describes a commercial jump between two stations
    /// </summary>
    public class TradeJumpData
    {
        internal TradeJumpData()
        {

        }

        internal TradeJumpData(Trade from, Trade to, int cargo = 1)
        {
            Fill(from, to, cargo);
        }

        internal void Fill(Trade from, Trade to, int cargo = 1)
        {
            From = from;
            To = to;
            Commodity = from.Commodity;
            ProfitPerUnit = to.BuyingPrice - from.SellingPrice;
            CargoSize = cargo;
            TotalProfit = ProfitPerUnit * cargo;
            TotalPrice = from.SellingPrice * cargo;
        }

        /// <summary>
        /// Gets the starting station of the jump
        /// </summary>
        public Trade From { get; internal set; }
        /// <summary>
        /// Gets the destination station of the jump
        /// </summary>
        public Trade To { get; internal set; }
        /// <summary>
        /// Gets the commodity that the trader should load during this jump
        /// </summary>
        public Commodity Commodity { get; internal set; }

        /// <summary>
        /// Gets the profit the trader should make for each loaded unit
        /// </summary>
        public float ProfitPerUnit { get; internal set; }
        /// <summary>
        /// Gets the amount of cargo that the <see cref="TotalProfit"/> and <see cref="TotalPrice"/> 
        /// properties were computed for.
        /// </summary>
        public int CargoSize { get; internal set; }
        
        /// <summary>
        /// Total profit the trader should make
        /// </summary>
        public float TotalProfit { get; internal set; }
        /// <summary>
        /// Total price the trader should pay to buy its cargo
        /// </summary>
        public float TotalPrice { get; internal set; }

        /// <summary>
        /// Gets the most ancient update time of all the data used to build this trading jump
        /// </summary>
        public DateTime DataDate
        {
            get
            {
                return From.DataDate > To.DataDate ? To.DataDate : From.DataDate;
            }
        }

        public override string ToString()
        {
            return String.Format("Trading {5} {0} between {1} ({2}) and {3} ({4})", Commodity.Name, From.Station.Name, From.SellingPrice, To.Station.Name, To.BuyingPrice, CargoSize);
        }
    }
}
