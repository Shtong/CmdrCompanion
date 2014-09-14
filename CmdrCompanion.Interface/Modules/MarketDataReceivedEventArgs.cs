using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.Modules
{
    public sealed class MarketDataReceivedEventArgs : EventArgs
    {
        public MarketDataReceivedEventArgs(
            float buyPrice,
            float sellPrice,
            int demand,
            int demandLevel,
            int stationStock,
            int stationStockLevel,
            string categoryName,
            string itemName,
            string stationName,
            string starName,
            DateTime timestamp)
        {
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            Demand = demand;
            DemandLevel = demandLevel;
            StationStock = stationStock;
            StationStockLevel = stationStockLevel;
            CategoryName = categoryName;
            ItemName = itemName;
            StationName = stationName;
            StarName = starName;
            Timestamp = timestamp;
        }

        public float BuyPrice { get; private set; }
        public float SellPrice { get; private set; }
        public int Demand { get; private set; }
        public int DemandLevel { get; private set; }
        public int StationStock { get; private set; }
        public int StationStockLevel { get; private set; }
        public string CategoryName { get; private set; }
        public string ItemName { get; private set; }
        public string StationName { get; private set; }
        public string StarName { get; private set; }
        public DateTime Timestamp { get; private set; }
    }
}
