using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.EmdnClient
{
    [JsonObject]
    internal sealed class MarketEntry
    {
        public string cateogryName = null;
        public float buyPrice = 0;
        public DateTime timestamp = new DateTime();
        public int stationStock = 0;
        public int stationStockLevel = 0;
        public string stationName = null;
        public int demand = 0;
        public float sellPrice = 0;
        public string itemName = null;
        public int demandLevel = 0;
    }
}
