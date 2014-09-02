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
            IsUpdating = true;

            Trace.TraceInformation("##### Starting with thread {0}. SynchronizationContext is {1}", Thread.CurrentThread.ManagedThreadId, SynchronizationContext.Current == null);

            string data = null;

            Task.Factory.StartNew(() =>
            {
                Trace.TraceInformation("###### Task is on thread {0}", Thread.CurrentThread.ManagedThreadId);

                WebClient client = new WebClient();
                try
                {
                    data = client.DownloadString("http://54.77.150.211/ED/PHP/dumpAll.php");
                }
                catch (WebException ex)
                {
                    Trace.TraceWarning("[SlopeyDB] Could not download data from the SLOPEY service: " + ex.Message);
                }
            }).ContinueWith(t =>
            {
                Trace.TraceInformation("###### Continuing on thread {0}", Thread.CurrentThread.ManagedThreadId);

                if (t.Exception != null)
                {
                    Trace.TraceError("Unhandled exception during EMDN data download !");
                    throw t.Exception;
                }

                if (data == null)
                {
                    Trace.TraceWarning("No data found after the EMDN download. Skipping this update");
                    return;
                }

                if (!data.StartsWith("ok"))
                {
                    Trace.TraceWarning("[SlopeyDB] Slopey service is NOT OK; aborting.");
                    return;
                }

                CultureInfo en = CultureInfo.GetCultureInfo(1033);

                // Remove the initial OK
                data = data.Substring(2);

                string[] lines = data.Split(new string[] { "<BR>" }, StringSplitOptions.RemoveEmptyEntries);

                int count = 0;
                foreach (string line in lines)
                {
                    // Line contents :
                    // UID,System, ,Station,Category,Commmodity, ,Selling,Buying,Stock,  ,  ,  ,  ,Date (07-Aug-2014 19:44:47),1
                    // 0    1     2     3   4           5       6   7       8       9  10 11 12 13  14                         15
                    string[] parts = line.Split(',');

                    // drop lines with an unsupported amount of values
                    if (parts.Length != 16)
                        continue;

                    float sellPrice = 0;
                    float buyPrice = 0;
                    int stock = 0;
                    if (!Single.TryParse(parts[7], out sellPrice))
                    {
                        Trace.TraceInformation("[SlopeyDB] Incorrect sell price ({0}) on entry {1}. Line skipped.", parts[7], parts[0]);
                        continue;
                    }
                    if (!Single.TryParse(parts[8], out buyPrice))
                    {
                        Trace.TraceInformation("[SlopeyDB] Incorrect buy price ({0}) on entry {1}. Line skipped.", parts[8], parts[0]);
                        continue;
                    }
                    if (!Int32.TryParse(parts[9], out stock))
                    {
                        Trace.TraceInformation("[SlopeyDB] Incorrect stock ({0}) on entry {1}. Line skipped.", parts[9], parts[0]);
                        continue;
                    }


                    Commodity com = Environment.FindCommodityByName(parts[5]);
                    // Check if the commodity already exists
                    if (com == null)
                    {
                        com = Environment.CreateCommodity(parts[5], parts[4]);
                        com.DataSourceName = DATA_SOURCE_NAME;
                    }

                    // Check if the system exists
                    Star star = Environment.FindStarByName(parts[1]);
                    if (star == null)
                    {
                        star = Environment.CreateStar(parts[1]);
                        star.DataSourceName = DATA_SOURCE_NAME;
                    }

                    // check if the station exists
                    Station station = star.FindStationByName(parts[3]);
                    if (station == null)
                    {
                        station = star.CreateStation(parts[3]);
                        station.DataSourceName = DATA_SOURCE_NAME;
                    }

                    // check if the trade exists
                    Trade trade = station.FindCommodity(com);
                    if (trade == null)
                    {
                        trade = station.CreateTrade(com, sellPrice, buyPrice, stock);
                    }
                    else
                    {
                        trade.Stock = stock;
                        trade.BuyingPrice = buyPrice;
                        trade.SellingPrice = sellPrice;
                    }
                    DateTime dt = DateTime.Now;
                    DateTime.TryParseExact(parts[14], "dd-MMM-yyyy HH:mm:ss", en, DateTimeStyles.None, out dt);
                    trade.DataDate = dt;
                    trade.DataSourceName = DATA_SOURCE_NAME;

                    ++count;
                }

                Trace.WriteLine(String.Format("[SlopeyDB] Updated {0} entries of trade data", count));
                IsUpdating = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void UpdateNow_FUTURE()
        {
            if (IsUpdating)
                return;

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
                                //JsonSerializer serializer = JsonSerializer.CreateDefault();
                                JsonSerializer serializer = null; // TODO : jdfmqljflmdqsj
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
                    throw t.Exception;
                }

                if(data == null)
                {
                    Trace.TraceWarning("No data found after the EMDN download. Skipping this update");
                    return;
                }

                foreach (MarketEntry entry in data)
                {
                    // Check if the commodity already exists
                    Commodity com = Environment.FindCommodityByName(entry.itemName);
                    if (com == null)
                    {
                        com = Environment.CreateCommodity(entry.itemName, entry.cateogryName);
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
}
