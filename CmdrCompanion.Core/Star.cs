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
    /// Contains informations about a star system
    /// </summary>
    /// <remarks>
    /// To create a Star instance, the best way is to use the <see cref="EliteEnvironment.CreateStar"/>
    /// in the <see cref="EliteEnvironment"/> instance the new star should belong to.
    /// </remarks>
    public class Star : CoreObject
    {
        internal Star(string name, EliteEnvironment environment)
        {
            Name = name;
            Environment = environment;
            _stations = new ObservableCollection<Station>();
            Stations = new ReadOnlyObservableCollection<Station>(_stations);
            KnownStarProximities = new StarProximityCollection();
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
                    throw new ArgumentException("Stars cannot have an empty name", "value");

                _name = value;
                OnPropertyChanged("Name");
            }
        }

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

        private ObservableCollection<Station> _stations;
        /// <summary>
        /// Gets a list of <see cref="Station"/> that can be found orbiting this star.
        /// </summary>
        /// <seealso cref="CreateStation"/>
        /// <seealso cref="FindStationByName"/>
        public ReadOnlyObservableCollection<Station> Stations { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Station"/> instance, and adds it to this star
        /// </summary>
        /// <param name="name">The name of the new station</param>
        /// <returns>The newly created station</returns>
        /// <exception cref="ArgumentNullException">The provided name is null.</exception>
        /// <exception cref="ArgumentException">The provided name is already in use within this star.</exception>
        public Station CreateStation(string name)
        {
            if(name == null)
                throw new ArgumentNullException("name", "A station name cannot be null");

            if (FindStationByName(name) != null)
                throw new ArgumentException("This station name already exists in this star.", "name");

            Station station = new Station(name, this);
            _stations.Add(station);
            Environment.InternalStations.Add(station);
            return station;
        }

        /// <summary>
        /// Finds a station using its name
        /// </summary>
        /// <param name="name">The station name to look for</param>
        /// <param name="comparisonOptions">The options used when comparing names</param>
        /// <returns>The <see cref="Station"/> instance that was found, or null if this name does not exist.</returns>
        public Station FindStationByName(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name", "A station name cannot be null");

            return _stations.Where(s => s.Name.Equals(name, comparisonOptions)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the <see cref="EliteEnvironment"/> into which this star is registered
        /// </summary>
        public EliteEnvironment Environment { get; private set; }

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
        public void RegisterDistanceFrom(Star otherStar, float distance)
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
        /// Returns the string representation of this instance
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return "Star system " + Name;
        }

        internal void Save(XmlWriter writer)
        {
            writer.WriteStartElement("star");
            writer.WriteAttributeString("name", Name);
            writer.WriteEndElement();
        }
    }
}
