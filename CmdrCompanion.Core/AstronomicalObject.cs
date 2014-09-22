using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// A basic astronomical object. While child classes can represent more
    /// precisely the object, this class can be used if the precise type of
    /// the object os not known.
    /// </summary>
    public class AstronomicalObject : CoreObject
    {
        // UNKNOWN
        // Star
        // Planet / Moon / Belt
        // Unidentified signals
        // Station

        internal AstronomicalObject(string name, EliteEnvironment environment, AstronomicalObjectType type, AstronomicalObject star)
        {
            Name = name;
            Environment = environment;

            ObjectsInternal = new ObservableCollection<AstronomicalObject>();
            Objects = new ReadOnlyObservableCollection<AstronomicalObject>(ObjectsInternal);
            KnownStarProximities = new StarProximityCollection();
            _trades = new ObservableCollection<Trade>();
            Trades = new ReadOnlyObservableCollection<Trade>(_trades);
            _commodityIndex = new Dictionary<Commodity, int>();

            if (environment.AutoDistanceEnabled)
            {
                float distance = 0;
                foreach (AstronomicalObject otherObject in environment.Objects)
                {
                    if (DistancesDB.TryGetDistance(this, otherObject, out distance))
                        RegisterDistanceFrom(otherObject, distance);
                }
            }


            Type = type;
            Star = star ?? this;

            Environment.ObjectsInternal.Add(this);
        }

        private string _name;
        /// <summary>
        /// Gets or sets the star's name
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Astronomical objects cannot have an empty name", "value");

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private AstronomicalObject _star;
        /// <summary>
        /// Gets the <see cref="Star"/> that this object orbits around.
        /// For stars, this is a self-reference.
        /// </summary>
        public AstronomicalObject Star
        {
            get
            {
                return _star;
            }
            internal set
            {
                if (_star != null && _star != this)
                    _star.ObjectsInternal.Remove(this);
                _star = value;
                if (_star != null && _star != this)
                    _star.ObjectsInternal.Add(this);


                OnPropertyChanged("Star");
            }
        }

        /// <summary>
        /// Gets the <see cref="EliteEnvironment"/> into which this object is registered
        /// </summary>
        public EliteEnvironment Environment { get; private set; }

        private AstronomicalObjectType _type;
        /// <summary>
        /// Gets or sets the type of this object
        /// </summary>
        public AstronomicalObjectType Type
        {
            get { return _type; }
            set
            {
                if(value != _type)
                {
                    if(_type == AstronomicalObjectType.Station)
                        Environment.StationsInternal.Remove(this);
                    if (_type == AstronomicalObjectType.Star)
                        Environment.StarsInternal.Remove(this);

                    _type = value;

                    if(_type == AstronomicalObjectType.Station)
                        Environment.StationsInternal.Add(this);
                    if (_type == AstronomicalObjectType.Star)
                        Environment.StarsInternal.Add(this);

                    
                    OnPropertyChanged("Type");

                }
            }
        }

        internal ObservableCollection<AstronomicalObject> ObjectsInternal { get; private set; }
        /// <summary>
        /// Gets a list of <see cref="AstronomicalObject"/> that can be found orbiting this object (directly or indirectly).
        /// </summary>
        /// <seealso cref="AstronomicalObject.CreateStation"/>
        /// <seealso cref="AstronomicalObject.CreateAstronomicalObject"/>
        /// <seealso cref="AstronomicalObject.CreateStar"/>
        /// <seealso cref="FindObjectByName"/>
        public ReadOnlyObservableCollection<AstronomicalObject> Objects { get; private set; }

        /// <summary>
        /// Gets a list of all other stars for which we know the distance from this star
        /// </summary>
        /// <remarks>
        /// <para>This is a dictionary where the keys are <see cref="Star"/> instances, and values
        /// are the distance with the current star. If another star is no present in this
        /// list, it means that its distance is not known.</para>
        /// <para>To register a distance between the current star and another, 
        /// use the <see cref="RegisterDistanceFrom"/> method.</para>
        /// </remarks>
        /// <seealso cref="RegisterDistanceFrom"/>
        public StarProximityCollection KnownStarProximities { get; private set; }

        /// <summary>
        /// Adds a new entry into the list of stars we know the distance from
        /// </summary>
        /// <param name="otherStar">Another <see cref="Star"/> instance.</param>
        /// <param name="distance">The distance between the current star and the 
        /// star specified in the <paramref name="otherStar"/> parameter.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="otherStar"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="otherStar"/> is equal to the current star</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="distance"/> parameter is negative 
        /// or equals zero.</exception>
        /// <remarks>
        /// This method can be called either to add a new distance, or to modify an existing distance. 
        /// The data will be added in the <see cref="KnownStarProximities"/> property of both the current star 
        /// and the star provided in the <paramref name="otherStar"/> parameter.
        /// </remarks>
        /// <seealso cref="KnownStarProximities"/>
        public void RegisterDistanceFrom(AstronomicalObject otherStar, float distance)
        {
            if (otherStar == null)
                throw new ArgumentNullException();

            if (otherStar == this)
                throw new ArgumentException("I cannot set a distance between me and myself !", "otherStar");

            if (distance <= 0)
                throw new ArgumentOutOfRangeException("Invalid distance", "distance");

            KnownStarProximities.Set(otherStar, distance);
            otherStar.KnownStarProximities.Set(this, distance);
        }

        /// <summary>
        /// Finds an object attached to this star, using its name
        /// </summary>
        /// <param name="name">The objet name to look for</param>
        /// <param name="comparisonOptions">The options used when comparing names</param>
        /// <returns>The <see cref="AstronomicalObject"/> instance that was found, or null if this name does not exist.</returns>
        public AstronomicalObject FindObjectByName(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A station name cannot be null");

            return ObjectsInternal.Where(o => o.Name.Equals(name, comparisonOptions)).FirstOrDefault();
        }

        /// <summary>
        /// Finds an object attached to this star, using its name
        /// </summary>
        /// <param name="name">The objet name to look for</param>
        /// <param name="comparisonOptions">The options used when comparing names</param>
        /// <returns>The <see cref="AstronomicalObject"/> instance that was found, or null if this name does not exist.</returns>
        public AstronomicalObject FindObjectByName(string name, AstronomicalObjectType type, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A station name cannot be null");

            return ObjectsInternal.Where(o => o.Name.Equals(name, comparisonOptions) && o.Type == type).FirstOrDefault();
        }


        public void Remove()
        {
            if (ObjectsInternal.Count > 0)
                throw new InvalidOperationException("You cannot remove an object that has other attached objects");

            if(Star != null)
                Star.ObjectsInternal.Remove(this);

            Environment.StationsInternal.Remove(this);
            Environment.ObjectsInternal.Remove(this);
        }

        /// <summary>
        /// Returns the string representation of this instance
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return String.Format("{0}: {1}", Type, Name);
        }

        internal virtual void Save(XmlWriter writer)
        {
            writer.WriteStartElement(Type.ToString().ToLower());
            writer.WriteAttributeString("name", Name);
            if(Star != null && Star != this)
                writer.WriteAttributeString("star", Star.Name);
            writer.WriteEndElement();
        }

        internal static bool Load(XmlReader reader, EliteEnvironment container)
        {
            if (!reader.IsStartElement())
                return false;

            AstronomicalObjectType type = AstronomicalObjectType.Unspecified;
            if (Enum.TryParse<AstronomicalObjectType>(reader.LocalName, true, out type))
            {
                string name = null;
                AstronomicalObject star = null;
                while(reader.MoveToNextAttribute())
                {
                    switch(reader.LocalName)
                    {
                        case "name":
                            name = reader.Value;
                            break;

                        case "star":
                            star = container.FindObjectByName(reader.Value, AstronomicalObjectType.Star);
                            if(star == null)
                                throw new EnvironmentLoadException(String.Format("The star {0} could not be found", reader.Value), reader);
                            break;
                    }
                }

                if (name == null)
                    throw new EnvironmentLoadException("Missing name for astronomical object entry", reader);


                AstronomicalObject result = new AstronomicalObject(name, container, type, star);
                return true;
            }
            else
                return false;
        }

        #region Market and trading
        private ObservableCollection<Trade> _trades;
        /// <summary>
        /// Gets a list of trading goods that can be exchenged at this station
        /// </summary>
        /// <seealso cref="CreateTrade"/>
        /// <seealso cref="RemoveTrade"/>
        /// <seealso cref="FindCommodity"/>
        public ReadOnlyObservableCollection<Trade> Trades { get; private set; }

        private Dictionary<Commodity, int> _commodityIndex;

        /// <summary>
        /// Creates a new <see cref="Trade"/> instance and adds it to this station
        /// </summary>
        /// <param name="commodity">The exchanged <see cref="Commodity"/></param>
        /// <param name="sellPrice">The unit price at which the station sells the commodity. Set to zero if the station does not sell.</param>
        /// <param name="buyPrice">The unit price at which the station buys the commodity. Set to zero if the station does not buy.</param>
        /// <param name="stock">The initial stock of goods, if the station is selling.</param>
        /// <returns>The newly created <see cref="Trade"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="commodity"/> parameter is null</exception>
        /// <exception cref="ArgumentException">The <paramref name="commodity"/> parameter is not know to the environment containing this station (see <see cref="EliteEnvironment.Commodities"/>)</exception>
        public Trade CreateTrade(Commodity commodity, float sellPrice, float buyPrice, int stock)
        {
            if (commodity == null)
                throw new ArgumentNullException("commodity");

            if (_commodityIndex.ContainsKey(commodity))
                throw new ArgumentException("This commodity is already available at this station", "commodity");

            Trade result = new Trade(this, commodity)
            {
                SellingPrice = sellPrice,
                BuyingPrice = buyPrice,
                Stock = stock,
            };
            _commodityIndex.Add(commodity, Trades.Count);
            _trades.Add(result);

            return result;
        }

        /// <summary>
        /// Removes a trade from this station
        /// </summary>
        /// <param name="commodity">The <see cref="Commodity"/> that should be removed from the trading list</param>
        /// <returns>true is the commodity was removed, false if it was not found in this station's trades.</returns>
        public bool RemoveTrade(Commodity commodity)
        {
            if (!_commodityIndex.ContainsKey(commodity))
                return false;

            _trades.RemoveAt(_commodityIndex[commodity]);
            _commodityIndex.Remove(commodity);

            return true;
        }

        /// <summary>
        /// Tries to find a <see cref="Trade"/> instance in this station's trades that matches the specified <see cref="Commodity"/>.
        /// </summary>
        /// <param name="commodity">The <see cref="Commodity"/> to look for</param>
        /// <returns>A <see cref="Trade"/> instance, or null if no trade was found.</returns>
        public Trade FindCommodity(Commodity commodity)
        {
            if (!_commodityIndex.ContainsKey(commodity))
                return null;
            return Trades[_commodityIndex[commodity]];
        }

        /// <summary>
        /// Returns a list of possible commodities that can be bought in this station and sold in another specified station.
        /// </summary>
        /// <param name="otherStation">A station to sell at</param>
        /// <returns>A list of <see cref="TradeJumpData"/> instances, describing the available trading options</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="otherStation"/> parameter is null</exception>
        /// <exception cref="ArgumentException">The <paramref name="otherStation"/> parameter equals the current station</exception>
        public List<TradeJumpData> FindTradesWith(AstronomicalObject otherStation)
        {
            if (otherStation == null)
                throw new ArgumentNullException("otherStation");

            if (otherStation == this)
                throw new ArgumentException("The otherStation argument cannot contain the Station you call the method on.", "otherStation");

            List<TradeJumpData> result = new List<TradeJumpData>();
            foreach (Trade t in Trades)
            {
                if (!t.CanSell)
                    continue;

                Trade t2 = otherStation.FindCommodity(t.Commodity);
                if (t2 == null || !t2.CanBuy)
                    continue;

                result.Add(new TradeJumpData(t, t2));
            }

            return result;
        }
        #endregion

        #region Creation tools
        /// <summary>
        /// Creates a new <see cref="Star"/> instance and adds it into this environment
        /// </summary>
        /// <param name="name">The name of the new star system</param>
        /// <param name="env">The environment that will contain the new star</param>
        /// <returns>The created <see cref="Star"/> instance.</returns>
        /// <exception cref="ArgumentNullException">The provided name is null</exception>
        /// <exception cref="ArgumentException">The provided name is already used by an existing star</exception>
        public static AstronomicalObject CreateStar(string name, EliteEnvironment env)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (env == null)
                throw new ArgumentNullException("name");

            AstronomicalObject result = null;
            AstronomicalObject existing = env.FindObjectByName(name);
            if (existing != null)
            {
                existing.Star = existing;
                existing.Type = AstronomicalObjectType.Star;
                result = existing;
            }
            else
            {
                result = new AstronomicalObject(name, env, AstronomicalObjectType.Star, null);
                result.Star = result;
            }

            return result;
        }

        /// <summary>
        /// Creates a new <see cref="Station"/> instance, and adds it the specified <see cref="Star"/>. If an object
        /// already exists in this star with the same name, it will be turned into a station.
        /// </summary>
        /// <param name="name">The name of the new station</param>
        /// <param name="star">The star system the new station belongs to</param>
        /// <returns>The newly created station</returns>
        /// <exception cref="ArgumentNullException">The provided name or container is null.</exception>
        /// <exception cref="ArgumentException">The provided name is already in use.</exception>
        public static AstronomicalObject CreateStation(string name, AstronomicalObject star)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (star == null)
                throw new ArgumentException("star");

            AstronomicalObject result = star.Environment.FindObjectByName(name);
            if (result == null)
            {
                result = new AstronomicalObject(name, star.Environment, AstronomicalObjectType.Station, star);
            }
            else
            {
                result.Star = star;
                result.Type = AstronomicalObjectType.Station;
            }

            return result;
        }

        /// <summary>
        /// Creates a new <see cref="AstronomicalObject"/> orbiting around this star.
        /// </summary>
        /// <param name="name">A name for the new object</param>
        /// <param name="star">The <see cref="Star"/> system that will contain the new object</param>
        /// <returns>The newly created <see cref="AstronomicalObject"/></returns>
        public static AstronomicalObject CreateAstronomicalObject(string name, AstronomicalObject star)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A star name cannot be null");

            AstronomicalObject result = star.Environment.FindObjectByName(name);
            if (result == null)
            {
                result = new AstronomicalObject(name, star.Environment, AstronomicalObjectType.NaturalObject, star);
            }
            else
            {
                result.Type = AstronomicalObjectType.NaturalObject;
                result.Star = star;
            }

            return result;
        }
        #endregion
    }
}
