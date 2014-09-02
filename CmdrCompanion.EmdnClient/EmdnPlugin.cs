using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CmdrCompanion.Core;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.IO;

namespace CmdrCompanion.EmdnClient
{
    [DataFeeder(DATA_FEEDER_NAME, Description="Gets trade data from the EMDN network")]
    public class EmdnPlugin : IAsyncDataFeeder
    {
        public const string DATA_FEEDER_NAME = "EMDN";

        public int Start(EliteEnvironment environment)
        {
            return 60;
        }

        public async Task<bool> Update(EliteEnvironment environment)
        {

            List<MarketEntry> data = null;
            try
            {
                await Task.Factory.StartNew(() => {
                    WebClient client = new WebClient();
                    using(Stream s = client.OpenRead(new Uri("http://emdn.sprods.net")))
                    {
                        using(StreamReader sReader = new StreamReader(s))
                        {
                            using(JsonReader jReader = new JsonTextReader(sReader))
                            {
                                //JsonSerializer serializer = JsonSerializer.CreateDefault();
                                JsonSerializer serializer = null; // TODO : jdfmqljflmdqsj
                                data = serializer.Deserialize<List<MarketEntry>>(jReader);
                            }
                        }
                    }
                });
            }
            catch(WebException ex)
            {
                Trace.TraceWarning("Could not download data from the EMDN relay: " + ex.Message);
                return true;
            }
            catch(Exception ex)
            {
                Trace.TraceError("An unexpected error occured while fetching / parsing the EMDN data !");
                Trace.TraceError(ex.ToString());
                return true;
            }

            foreach(MarketEntry entry in data)
            {
                // Check if the commodity already exists
                Commodity com = environment.FindCommodityByName(entry.itemName);
                if (com == null)
                {
                    com = environment.CreateCommodity(entry.itemName, entry.cateogryName);
                    com.DataSourceName = DATA_FEEDER_NAME;
                }

                // Split the star name and station name
                string[] parts = entry.stationName.Split('(');
                string starName = parts[1].Substring(0, parts[1].Length - 1);
                string stationName = parts[0].Substring(0, parts[0].Length - 1);

                // Check if the star already exists
                Star star = environment.FindStarByName(starName);
                if (star == null)
                {
                    star = environment.CreateStar(starName);
                    star.DataSourceName = DATA_FEEDER_NAME;
                }

                // Check if the station exists
                Station station = star.FindStationByName(stationName);
                if (station == null)
                {
                    station = star.CreateStation(stationName);
                    station.DataSourceName = DATA_FEEDER_NAME;
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
                trade.DataSourceName = DATA_FEEDER_NAME;
            }

            return true;
        }
    }
}
