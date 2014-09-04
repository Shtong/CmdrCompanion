using CmdrCompanion.Core;
using CmdrCompanion.Interface.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CmdrCompanion.Interface.Modules
{
    internal class EmdnUpdater
    {
        public const string DATA_SOURCE_NAME = "EMDN";

        public EmdnUpdater(EliteEnvironment env)
        {
            if (env == null)
                throw new ArgumentNullException("env");

            Environment = env;

            Timer = new DispatcherTimer(new TimeSpan(0, 1, 0), DispatcherPriority.Background, (sender, e) => UpdateNow(), Dispatcher.CurrentDispatcher);
            Timer.Stop(); // DispatcherTimer is enabled by default -_-
        }

        public EliteEnvironment Environment { get; private set; }

        public bool IsEnabled
        {
            get
            {
                return Timer.IsEnabled;
            }
        }

        public bool IsUpdating { get; private set; }

        private DispatcherTimer Timer { get; set; }

        public void Enable()
        {
            if (IsEnabled)
                return;

            UpdateNow();
            Timer.Start();
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            Timer.Stop();
        }

        public void UpdateNow()
        {
            if (IsUpdating)
                return;

            Trace.TraceInformation("Updating");

            IsUpdating = true;


            List<MarketEntry> data = null;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    WebClient client = new WebClient();
                    using (Stream s = client.OpenRead(new Uri("http://emdn.sprods.net")))
                    {
                        using (StreamReader sReader = new StreamReader(s))
                        {
                            using (JsonReader jReader = new JsonTextReader(sReader))
                            {
                                JsonSerializer serializer = JsonSerializer.CreateDefault();
                                data = serializer.Deserialize<List<MarketEntry>>(jReader);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Trace.TraceWarning("Could not download data from the EMDN relay: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("An unexpected error occured while fetching / parsing the EMDN data !");
                    Trace.TraceError(ex.ToString());
                }
            }).ContinueWith((t) => {
                if (t.Exception != null)
                {
                    Trace.TraceError("Unhandled exception during EMDN data download !");
                    Trace.TraceError(t.Exception.ToString());
                    Trace.TraceError(t.Exception.StackTrace);
                    return;
                }

                if(data == null)
                {
                    Trace.TraceWarning("No data found after the EMDN download. Skipping this update");
                    return;
                }

                foreach (MarketEntry entry in data)
                {
                    if (entry.categoryDisplayName == null)
                        entry.categoryDisplayName = entry.categoryName;

                    if (entry.itemDisplayName == null)
                        entry.itemDisplayName = entry.itemName;

                    // Check if the commodity already exists
                    Commodity com = Environment.FindCommodityByName(entry.itemDisplayName);
                    if (com == null)
                    {
                        com = Environment.CreateCommodity(entry.itemDisplayName, entry.categoryDisplayName);
                        com.DataSourceName = DATA_SOURCE_NAME;
                    }

                    // Split the star name and station name
                    string[] parts = entry.stationName.Split('(');
                    string starName = parts[1].Substring(0, parts[1].Length - 1);
                    string stationName = parts[0].Substring(0, parts[0].Length - 1);

                    // Check if the star already exists
                    Star star = Environment.FindStarByName(starName);
                    if (star == null)
                    {
                        star = Environment.CreateStar(starName);
                        star.DataSourceName = DATA_SOURCE_NAME;
                    }

                    // Check if the station exists
                    Station station = star.FindStationByName(stationName);
                    if (station == null)
                    {
                        station = star.CreateStation(stationName);
                        station.DataSourceName = DATA_SOURCE_NAME;
                    }

                    // check if the trade exists
                    Trade trade = station.FindCommodity(com);
                    if (trade == null)
                    {
                        trade = station.CreateTrade(com, entry.sellPrice, entry.buyPrice, entry.stationStock);
                    }
                    else
                    {
                        trade.Stock = entry.stationStock;
                        trade.BuyingPrice = entry.buyPrice;
                        trade.SellingPrice = entry.sellPrice;
                    }
                    trade.DataDate = entry.timestamp;
                    trade.DataSourceName = DATA_SOURCE_NAME;

                    IsUpdating = false;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private class MarketEntry
        {
            public string categoryName = null;
            public float buyPrice = 0;
            public DateTime timestamp = new DateTime();
            public int stationStock = 0;
            public int stationStockLevel = 0;
            public string stationName = null;
            public int demand = 0;
            public float sellPrice = 0;
            public string itemName = null;
            public int demandLevel = 0;
            public string itemDisplayName = null;
            public string categoryDisplayName = null;
        }
    }
}
