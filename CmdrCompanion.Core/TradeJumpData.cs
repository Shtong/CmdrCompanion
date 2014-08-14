using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
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

        public Trade From { get; internal set; }
        public Trade To { get; internal set; }
        public Commodity Commodity { get; internal set; }

        public float ProfitPerUnit { get; internal set; }
        public int CargoSize { get; internal set; }
        
        public float TotalProfit { get; internal set; }
        public float TotalPrice { get; internal set; }

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
