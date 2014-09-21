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
    public class Star : AstronomicalObject
    {
        internal Star(string name, EliteEnvironment environment)
            : base(name, environment)
        {
            ObjectsInternal = new ObservableCollection<AstronomicalObject>();
            Objects = new ReadOnlyObservableCollection<AstronomicalObject>(ObjectsInternal);
            KnownStarProximities = new StarProximityCollection();
        }

        internal Star(AstronomicalObject source)
            : base(source)
        {

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

        internal ObservableCollection<AstronomicalObject> ObjectsInternal { get; private set; }
        /// <summary>
        /// Gets a list of <see cref="AstronomicalObject"/> that can be found orbiting this star.
        /// </summary>
        /// <seealso cref="CreateStation"/>
        /// <seealso cref="CreateAstronomicalObject"/>
        /// <seealso cref="FindObjectByName"/>
        public ReadOnlyObservableCollection<AstronomicalObject> Objects { get; private set; }


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
        /// Finds an object orbiting this star with the specified name and type
        /// </summary>
        /// <typeparam name="T">The type of the object to look for</typeparam>
        /// <param name="name">The name of the object to look for</param>
        /// <param name="comparisonOptions">The options used when comparing names</param>
        /// <returns>An instance of the object that was found, or <c>null</c> if nothing matched.</returns>
        public T FindObjectByName<T>(string name, StringComparison comparisonOptions = StringComparison.InvariantCultureIgnoreCase) 
            where T : AstronomicalObject
        {
            if (name == null)
                throw new ArgumentNullException("name", "An astronomical object name cannot be null");

            return (T)ObjectsInternal.Where(o => o.Name.Equals(name, comparisonOptions) && o.GetType() == typeof(T)).FirstOrDefault();
        }

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

        internal override void Remove()
        {
            if (ObjectsInternal.Count > 0)
                throw new InvalidOperationException("You cannot remove a star that has attached objects");

            Environment.ObjectsInternal.Remove(this);
            Environment.StarsInternal.Remove(this);
        }

        /// <summary>
        /// Returns the string representation of this instance
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return "Star " + Name;
        }

        internal override void Save(XmlWriter writer)
        {
            writer.WriteStartElement("star");
            writer.WriteAttributeString("name", Name);
            writer.WriteEndElement();
        }

        internal static new void Load(XmlReader reader, EliteEnvironment container)
        {
            string name = null;

            while(reader.MoveToNextAttribute())
            {
                switch(reader.LocalName)
                {
                    case "name":
                        name = reader.Value;
                        break;
                }
            }

            if (name == null)
                throw new EnvironmentLoadException("No name found while loading a star entry", reader);

            AstronomicalObject.CreateStar(name, container);

            reader.Read();
        }
    }
}
