using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Globalization;

namespace CmdrCompanion.Slopey
{
    [DataFeeder(SlopeyDB.DATA_FEEDER_NAME, Description="Gets trade data from Slopey's BPC online database")]
    public class SlopeyDB : IAsyncDataFeeder
    {
        public const string DATA_FEEDER_NAME = "SlopeyDB";

        private int _distUpdateCounter;

        public int Start(EliteEnvironment environment)
        {
            Trace.WriteLine("[SlopeyDB] Starting");
            return 60;
        }

        public async Task<bool> Update(EliteEnvironment environment)
        {
            await UpdateTradeData(environment);

            if(_distUpdateCounter <= 0)
            {
                await UpdateDistances(environment);
                _distUpdateCounter = 10;
            }
            --_distUpdateCounter;
            return true;
        }

        private async Task UpdateTradeData(EliteEnvironment environment)
        {
            WebClient client = new WebClient();
            string data = null;

            try
            {
                data = await client.DownloadStringTaskAsync("http://54.77.150.211/ED/PHP/dumpAll.php");
            }
            catch (WebException ex)
            {
                Trace.TraceWarning("[SlopeyDB] Could not download data from the SLOPEY service: " + ex.Message);
            }

            if (data == null)
                return;

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


                Commodity com = environment.FindCommodityByName(parts[5]);
                // Check if the commodity already exists
                if (com == null)
                {
                    com = environment.CreateCommodity(parts[5], parts[4]);
                    com.DataSourceName = DATA_FEEDER_NAME;
                }

                // Check if the system exists
                Star star = environment.FindStarByName(parts[1]);
                if (star == null)
                {
                    star = environment.CreateStar(parts[1]);
                    star.DataSourceName = DATA_FEEDER_NAME;
                }

                // check if the station exists
                Station station = star.FindStationByName(parts[3]);
                if (station == null)
                {
                    station = star.CreateStation(parts[3]);
                    station.DataSourceName = DATA_FEEDER_NAME;
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
                trade.DataSourceName = DATA_FEEDER_NAME;

                ++count;
            }

            Trace.WriteLine(String.Format("[SlopeyDB] Updated {0} entries of trade data", count));
        }

        private async Task UpdateDistances(EliteEnvironment environment)
        {
            WebClient client = new WebClient();
            string data = null;

            try
            {
                data = await client.DownloadStringTaskAsync("http://54.77.150.211/ED/PHP/dumpAll-Distance.php");
            }
            catch(WebException ex)
            {
                Trace.TraceWarning("[SlopeyDB] Could not download the distances data: " + ex.Message);
            }

            if (data == null)
                return;

            string[] lines = data.Split(new string[] { "<BR>" }, StringSplitOptions.RemoveEmptyEntries);
            float distance;
            int count = 0;
            foreach(string line in lines)
            {
                string[] parts = line.Split(',');

                // id, from star name, to star name, distance, ?
                // 0        1               2           3       4

                if (parts.Length != 5)
                    continue;

                Star s1 = environment.FindStarByName(parts[1]);
                if(s1 != null)
                {
                    Star s2 = environment.FindStarByName(parts[2]);
                    if(s2 != null 
                        && s1 != s2
                        && Single.TryParse(parts[3], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowThousands | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo, out distance)
                        && distance > 0)
                    {
                        s1.RegisterDistanceFrom(s2, distance);
                    }
                }

                ++count;
            }

            Trace.WriteLine(String.Format("[SlopeyDB] Updated {0} entries of distance data", count));
        }
    }
}
