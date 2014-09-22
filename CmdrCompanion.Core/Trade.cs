using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Describes the commercial availability of a <see cref="Commodity"/>
    /// </summary>
    /// <seealso cref="CmdrCompanion.Core.Station.CreateTrade"/>
    public class Trade : CoreObject
    {
        internal Trade(AstronomicalObject station, Commodity commodity)
        {
            Station = station;
            Commodity = commodity;
            DataDate = DateTime.Now;
        }

        /// <summary>
        /// Gets a value indicating whether the station has this commodity available for sale
        /// </summary>
        public bool CanSell
        { 
            get
            {
                return SellingPrice > 0 && Stock > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the station can buy this commodity
        /// </summary>
        public bool CanBuy 
        { 
            get
            {
                return BuyingPrice > 0;
            }
        }

        private float _sellingPrice;
        /// <summary>
        /// Gets of sets the price at which the station will sell this commodity to traders
        /// </summary>
        public float SellingPrice 
        { 
            get
            {
                return _sellingPrice;
            }

            set
            {
                if (value < 0)
                    value = 0;

                if (value != _sellingPrice)
                {
                    _sellingPrice = value;
                    OnPropertyChanged(new string[] { "SellingPrice", "CanSell" });
                }
            }
        }

        private float _buyingPrice;
        /// <summary>
        /// Gets of sets the price at which the station will buy this commodity from traders
        /// </summary>
        public float BuyingPrice 
        { 
            get
            {
                return _buyingPrice;
            }

            set
            {
                if (value < 0)
                    value = 0;

                if(value != _buyingPrice)
                {
                    _buyingPrice = value;
                    OnPropertyChanged(new string[] { "BuyingPrice", "CanBuy" });
                }
            }
        }

        private int _stock;
        /// <summary>
        /// Gets of sets the amount of units available for sale
        /// </summary>
        public int Stock 
        { 
            get
            {
                return _stock;
            }

            set
            {
                if (value < 0)
                    value = 0;

                if(value != _stock)
                { 
                    _stock = value;
                    OnPropertyChanged(new string[] { "Stock", "CanSell" });
                }
            }
        }

        /// <summary>
        /// Gets the station that conducts this trade.
        /// </summary>
        public AstronomicalObject Station { get; private set; }

        /// <summary>
        /// Gets the commodity that this trade instance is about
        /// </summary>
        public Commodity Commodity { get; private set; }

        private DateTime _dataDate;
        /// <summary>
        /// Gets or sets the date at which the data in this instance was extracted from the game
        /// </summary>
        public DateTime DataDate
        {
            get
            {
                return _dataDate;
            }

            set
            {
                if(value != _dataDate)
                {
                    _dataDate = value;
                    OnPropertyChanged("DataDate");
                }
            }
        }

        /// <summary>
        /// Returns the string representation of this instance
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return String.Format("Trade of {0} between at station {1}", Commodity.Name, Station.Name);
        }

        internal void Save(XmlWriter writer)
        {
            writer.WriteStartElement("trade");
            writer.WriteAttributeString("commodity", Commodity.Name);
            writer.WriteAttributeFloat("sellingPrice", SellingPrice);
            writer.WriteAttributeFloat("buyingPrice", BuyingPrice);
            writer.WriteAttributeInt("stock", Stock);
            writer.WriteAttributeString("station", Station.Name);
            
            writer.WriteEndElement();
        }

        internal static bool Load(XmlReader reader, EliteEnvironment container)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "trade")
                return false;

            Commodity commodity = null;
            AstronomicalObject station = null;
            float sellingPrice = 0;
            float buyingPrice = 0;
            int stock = 0;

            while(reader.MoveToNextAttribute())
            {
                switch(reader.LocalName)
                {
                    case "commodity":
                        commodity = container.FindCommodityByName(reader.Value);
                        if(commodity == null)
                            throw new EnvironmentLoadException(String.Format("Unknown commodity name '{0}'", reader.Value), reader);
                        break;

                    case "station":
                        station = container.Stations.Where(s => s.Name == reader.Value).FirstOrDefault();
                        if (station == null)
                            throw new EnvironmentLoadException(String.Format("Unknown station name '{0}'", reader.Value), reader);
                        break;

                    case "sellingPrice":
                        sellingPrice = reader.ReadFloat();
                        break;

                    case "buyingPrice":
                        buyingPrice = reader.ReadFloat();
                        break;

                    case "stock":
                        stock = reader.ReadInt();
                        break;
                }
            }

            if (commodity == null)
                throw new EnvironmentLoadException("Missing commodity for a trade entry", reader);

            if (station == null)
                throw new EnvironmentLoadException("Missing station for a trade entry", reader);

            station.CreateTrade(commodity, sellingPrice, buyingPrice, stock);

            reader.Read();
            return true;
        }
    }
}
