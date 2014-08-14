using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public class EliteEnvironment : CoreObject
    {
        public EliteEnvironment()
        {
            Stars = new ObservableCollection<Star>();

            InternalCommodities = new ObservableCollection<Commodity>();
            Commodities = new ReadOnlyObservableCollection<Commodity>(InternalCommodities);

            InternalStations = new ObservableCollection<Station>();
            Stations = new ReadOnlyObservableCollection<Station>(InternalStations);
        }

        public ObservableCollection<Star> Stars { get; private set; }

        internal ObservableCollection<Station> InternalStations { get; private set; }
        /// <summary>
        /// Gets a list of all the stations in this environment
        /// </summary>
        public ReadOnlyObservableCollection<Station> Stations { get; set; }

        internal ObservableCollection<Commodity> InternalCommodities { get; private set; }
        public ReadOnlyObservableCollection<Commodity> Commodities { get; private set; }

        public Star FindStarByName(string name)
        {
            return Stars.Where(s => s.Name == name).FirstOrDefault();
        }

        public Star CreateStar(string name)
        {
            Star result = new Star(name, this);
            Stars.Add(result);
            return result;
        }

        public Commodity FindCommodityByName(string name)
        {
            return InternalCommodities.Where(c => c.Name == name).FirstOrDefault();
        }

        public Commodity CreateCommodity(string name, string category = null)
        {
            Commodity result = new Commodity(name, category);
            InternalCommodities.Add(result);
            return result;
        }

        /// <summary>
        /// Returns the standard Elite:Dangerous time
        /// </summary>
        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow.AddYears(1286);
            }
        }

        public TradeJumpData FindBestProfit(Station from, Station to, int cargo, int budget)
        {
            if (from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");
            if (from == to)
                throw new ArgumentException("The from and to stations cannot be the same");

            TradeJumpData result = new TradeJumpData();
            result.TotalProfit = Single.MinValue;

            foreach(Trade t in from.Trades)
            {
                if(!t.CanSell)
                    continue;

                Trade t2 = to.FindCommodity(t.Commodity);
                if (t2 == null || !t2.CanBuy)
                    continue;

                float profitPerUnit = t2.BuyingPrice - t.SellingPrice;
                int cargoSize = Math.Min(cargo, (int)(budget / t.SellingPrice));
                float totalProfit = profitPerUnit * cargoSize;

                if(totalProfit > result.TotalProfit)
                    result.Fill(t, t2, cargoSize);
            }

            if (result.Commodity == null)
                return null;
            return result;
        }
    }
}
