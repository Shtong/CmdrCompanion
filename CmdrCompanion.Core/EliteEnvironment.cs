using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// This class is used as the root access point for all the data of a CmdrCompanion session.
    /// </summary>
    public class EliteEnvironment : CoreObject
    {
        private const string DEEP_SPACE_NAME = "Deep Space";

        /// <summary>
        /// Creates a new instance of <see cref="EliteEnvironment"/>.
        /// </summary>
        public EliteEnvironment()
        {
            AutoDistanceEnabled = true;

            ObjectsInternal = new ObservableCollection<AstronomicalObject>();
            Objects = new ReadOnlyObservableCollection<AstronomicalObject>(ObjectsInternal);

            StationsInternal = new ObservableCollection<Station>();
            Stations = new ReadOnlyObservableCollection<Station>(StationsInternal);

            StarsInternal = new ObservableCollection<Star>();
            Stars = new ReadOnlyObservableCollection<Star>(StarsInternal);

            CommoditiesInternal = new ObservableCollection<Commodity>();
            Commodities = new ReadOnlyObservableCollection<Commodity>(CommoditiesInternal);

            CurrentSituation = new Situation();
        }

        internal ObservableCollection<AstronomicalObject> ObjectsInternal { get; private set; }

        /// <summary>
        /// Gets a list of all the known stars
        /// </summary>
        /// <seealso cref="AstronomicalObject.CreateStar"/>
        public ReadOnlyObservableCollection<AstronomicalObject> Objects { get; private set; }

        internal ObservableCollection<Station> StationsInternal { get; private set; }
        /// <summary>
        /// Gets a list of all the known stations
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is actually a shortcut to the list of stations contained in the <see cref="Objects"/>
        /// of this environment.
        /// </para>
        /// </remarks>
        public ReadOnlyObservableCollection<Station> Stations { get; set; }

        internal ObservableCollection<Star> StarsInternal { get; private set; }
        /// <summary>
        /// Gets a list of all the known stations
        /// </summary>
        public ReadOnlyObservableCollection<Star> Stars { get; private set; }

        internal ObservableCollection<Commodity> CommoditiesInternal { get; private set; }
        /// <summary>
        /// Gets a list of all known tradable commodities
        /// </summary>
        /// <seealso cref="CreateCommodity"/>
        public ReadOnlyObservableCollection<Commodity> Commodities { get; private set; }

        /// <summary>
        /// Finds a specific star based on its name
        /// </summary>
        /// <param name="name">The name of a star</param>
        /// <param name="comparisonOptions">Options for comparing the star names</param>
        /// <returns>The <see cref="Star"/> instance that was found, or null if no star could be found with that name.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        public AstronomicalObject FindObjectByName(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "An astronomical object name cannot be null");

            return Objects.Where(s => s.Name.Equals(name, comparisonOptions)).FirstOrDefault();
        }

        /// <summary>
        /// Finds a specific star based on its name and its type
        /// </summary>
        /// <typeparam name="T">The type of an astronomical object</typeparam>
        /// <param name="name">The name of a star</param>
        /// <param name="comparisonOptions">Options for comparing the star names</param>
        /// <returns>The <see cref="Star"/> instance that was found, or null if no star could be found with that name.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
       public T FindObjectByName<T>(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
            where T: AstronomicalObject
        {
            if (name == null)
                throw new ArgumentNullException("name", "An astronomical object name cannot be null");

            return (T)Objects.Where(o => o.Name.Equals(name, comparisonOptions) && o.GetType() == typeof(T)).FirstOrDefault();
        }

        /// <summary>
        /// Finds a commodity that has the sspecified name
        /// </summary>
        /// <param name="name">A commodity name to look for</param>
        /// <param name="comparisonOptions">Options used to compare the commodity names</param>
        /// <returns>The <see cref="Commodity"/> that was found, or null if the provided name does not match any commodity.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        public Commodity FindCommodityByName(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A commodity name cannot be null");

            return CommoditiesInternal.Where(c => c.Name.Equals(name, comparisonOptions)).FirstOrDefault();
        }

        /// <summary>
        /// Creates a new <see cref="Commodity"/> instance and adds it into this environment.
        /// If this commodity name already exists, the existing commodity is returned.
        /// </summary>
        /// <param name="name">The name of the new commodity</param>
        /// <param name="category">An opptionnal category name</param>
        /// <returns>The newly created <see cref="Commodity"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        public Commodity CreateCommodity(string name, string category = null)
        {
            if (name == null)
                throw new ArgumentNullException("name", "The commodity name cannot be null");

            Commodity existing = FindCommodityByName(name);
            if (existing != null)
                return existing;

            Commodity result = new Commodity(name, category);
            CommoditiesInternal.Add(result);
            return result;
        }

        /// <summary>
        /// Gets the standard Elite:Dangerous time
        /// </summary>
        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow.AddYears(1286);
            }
        }

        /// <summary>
        /// Computes the most profitable trading jump given the specified constraints
        /// </summary>
        /// <param name="from">The <see cref="Station"/> to buy the goods from</param>
        /// <param name="to">The <see cref="Station"/> to sell the goods at</param>
        /// <param name="cargo">The maximum amount of units that can be bought</param>
        /// <param name="budget">The maximum price that can be paid for buying the goods</param>
        /// <returns>A <see cref="TradeJumpData"/> instance describing the ideal trade operation</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="from"/> or <paramref name="to"/> arguments are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        ///     <item>
        ///         <description>The <paramref name="cargo"/> parameter is negative or zero.</description>
        ///     </item>
        ///     <item>
        ///         <description>The <paramref name="budget"/> parameter is negative or zero.</description>
        ///     </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">The <paramref name="from"/> and <paramref name="to"/> parameters are equal</exception>
        public TradeJumpData FindBestProfit(Station from, Station to, int cargo, int budget)
        {
            if (from == null)
                throw new ArgumentNullException("from");
            if (to == null)
                throw new ArgumentNullException("to");
            if (from == to)
                throw new ArgumentException("The from and to stations cannot be the same");
            if (cargo <= 0)
                throw new ArgumentOutOfRangeException("cargo");
            if (budget <= 0)
                throw new ArgumentOutOfRangeException("budget");

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

        private bool _autoDistanceEnabled;
        /// <summary>
        /// Gets or sets whether all distances are automatically filled out when adding a new star in this system.
        /// </summary>
        public bool AutoDistanceEnabled
        {
            get { return _autoDistanceEnabled; }
            set
            {
                if(value != _autoDistanceEnabled)
                {
                    _autoDistanceEnabled = value;
                    OnPropertyChanged("AutoDistanceEnabled");
                }
            }
        }

        /// <summary>
        /// Saves this environment's data into the specified stream
        /// </summary>
        /// <param name="s">The stream to save the data to</param>
        /// <seealso cref="Load"/>
        public void Save(Stream s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            if (!s.CanWrite)
                throw new ArgumentException("The stream must be writable");

            XmlWriterSettings xmlSettings = new XmlWriterSettings()
            {
                CloseOutput = false,
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineHandling = NewLineHandling.Entitize,
            };
            using(XmlWriter writer = XmlWriter.Create(s, xmlSettings))
            {
                writer.WriteStartElement("environment");
                writer.WriteAttributeDate("date", DateTime.Now);
                //writer.WriteAttributeBool("autodistance", AutoDistanceEnabled);
                if (CurrentSituation.CurrentLocation != null)
                    writer.WriteAttributeString("current", CurrentSituation.CurrentLocation.Name);

                writer.WriteStartElement("commodities");
                foreach (Commodity c in CommoditiesInternal)
                    c.Save(writer);
                writer.WriteEndElement();

                writer.WriteStartElement("objects");
                // Stars first
                foreach(Star star in StarsInternal)
                    star.Save(writer);
                // Others next
                foreach (AstronomicalObject ao in ObjectsInternal.Where(ao => ao.GetType() != typeof(Star)))
                    ao.Save(writer);
                writer.WriteEndElement();

                writer.WriteStartElement("trades");
                foreach (Station station in StationsInternal)
                    foreach (Trade t in station.Trades)
                        t.Save(writer);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.Flush();
            }
        }

        /// <summary>
        /// Loads an environment from the specified stream.
        /// </summary>
        /// <param name="source">A stream containing data previously written by the <see cref="Save"/> method.</param>
        /// <exception cref="ArgumentNullException">The provided stream is null</exception>
        /// <exception cref="ArgumentException">The provided stream cannot be read</exception>
        /// <exception cref="EnvironmentLoadException">The stream contained invalid data</exception>
        public void Load(Stream source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (!source.CanRead)
                throw new ArgumentException("Source stream must be readable", "source");

            XmlReaderSettings xmlSettings = new XmlReaderSettings()
            {
                CloseInput = false,
            };
            using(XmlReader reader = XmlReader.Create(source, xmlSettings))
            {
                string currentName = null;

                while(reader.Read())
                {
                    // Move to the root element
                    if (reader.IsStartElement())
                        break;
                }
                while(reader.MoveToNextAttribute())
                {
                    switch (reader.LocalName)
                    {
                        //case "autodistance":
                        //    AutoDistanceEnabled = reader.ReadBool();
                        //    break;

                        case "current":
                            currentName = reader.Value;
                            break;
                    }
                }

                while(reader.Read())
                {
                    if(reader.IsStartElement())
                    {
                        switch(reader.LocalName)
                        {
                            case "commodities":
                                // Using a subtree reader ensures that the rest of the loading
                                // code will not get past the end of the container, and
                                // that we are positionned at the end of the container after
                                // the reading is done
                                using (XmlReader commoditiesReader = reader.ReadSubtree())
                                {
                                    while(commoditiesReader.Read())
                                    {
                                        if(commoditiesReader.IsStartElement())
                                            Commodity.Load(commoditiesReader, this);
                                    }
                                }
                                break;

                            case "objects":
                                using(XmlReader objectsReader = reader.ReadSubtree())
                                {
                                    objectsReader.Read();
                                    while(objectsReader.Read())
                                    {
                                        if (objectsReader.IsStartElement())
                                        {
                                            if (!AstronomicalObject.Load(reader, this))
                                                break;
                                        }
                                    }
                                }
                                break;

                            case "trades":
                                using(XmlReader tradesReader = reader.ReadSubtree())
                                {
                                    while(tradesReader.Read())
                                    {
                                        if (tradesReader.IsStartElement())
                                            Trade.Load(tradesReader, this);
                                    }
                                }
                                break;
                        }
                    }
                }

                if (currentName != null)
                    CurrentSituation.CurrentLocation = FindObjectByName(currentName);
            }
        }

        /// <summary>
        /// Gets an object that describes the player's current situation
        /// </summary>
        public Situation CurrentSituation
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the <see cref="AstronomicalObject"/> singleton that is used to represent deep space locations,
        /// moving it to the specified star
        /// </summary>
        /// <param name="moveToStar">The star that the deep space location is near to</param>
        /// <returns>A <see cref="DeepSpace"/> instance</returns>
        public DeepSpace GetDeepSpaceObject(Star moveToStar)
        {
            DeepSpace result = (DeepSpace)FindObjectByName(DEEP_SPACE_NAME, StringComparison.InvariantCulture);

            if(result == null)
                result = new DeepSpace(DEEP_SPACE_NAME, moveToStar, false, false);
            else
                result.Star = moveToStar;

            return result;
        }
    }
}
