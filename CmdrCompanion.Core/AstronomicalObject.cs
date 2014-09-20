using System;
using System.Collections.Generic;
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

        internal AstronomicalObject(string name, EliteEnvironment environment, Star star = null)
        {
            Name = name;
            Environment = environment;

            if (star == null)
            {
                if (this is Star)
                    Star = (Star)this;
                else
                    throw new InvalidOperationException("Please provide a star for non-star objects");
            }
            else
                Star = star;
        }

        internal AstronomicalObject(AstronomicalObject source)
        {
            Name = source.Name;
            Star = source.Star;
            Environment = source.Environment;
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

        private Star _star;
        /// <summary>
        /// Gets the <see cref="Star"/> that this station orbits around.
        /// </summary>
        public Star Star
        {
            get
            {
                return _star;
            }
            internal set
            {
                _star = value;

                OnPropertyChanged("Star");
            }
        }

        /// <summary>
        /// Gets the <see cref="EliteEnvironment"/> into which this star is registered
        /// </summary>
        public EliteEnvironment Environment { get; private set; }

        internal virtual void Remove()
        {
            Environment.ObjectsInternal.Remove(this);
            Star.ObjectsInternal.Remove(this);
        }

        /// <summary>
        /// Returns the string representation of this instance
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return "Astronomical object: " + Name;
        }

        internal virtual void Save(XmlWriter writer)
        {
            WriteStartElement(writer, "astronomicalobject");
            writer.WriteEndElement();
        }

        protected void WriteStartElement(XmlWriter writer, string elementName)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString("name", Name);
            writer.WriteAttributeString("star", Star.Name);
        }

        internal static bool Load(XmlReader reader, EliteEnvironment container)
        {
            if (!reader.IsStartElement())
                return false;

            switch(reader.LocalName)
            {
                case "object":
                    LoadBasicObject(reader, container);
                    return true;
                case "star":
                    Star.Load(reader, container);
                    return true;
                case "station":
                    Station.Load(reader, container);
                    return true;
                default:
                return false;
            }
        }

        private static void LoadBasicObject(XmlReader reader, EliteEnvironment container)
        {
            string name = null;
            Star star = null;
            while(reader.MoveToNextAttribute())
            {
                switch(reader.LocalName)
                {
                    case "name":
                        name = reader.Value;
                        break;

                    case "star":
                        star = container.FindObjectByName<Star>(reader.Value);
                        if(star == null)
                            throw new EnvironmentLoadException(String.Format("The star {0} could not be found", reader.Value), reader);
                        break;
                }
            }

            if (name == null)
                throw new EnvironmentLoadException("Missing name for astronomical object entry", reader);

            if(star == null)
                throw new EnvironmentLoadException("Missing star attribute for a astronomical object entry", reader);

            star.CreateAstronomicalObject(name);
            reader.Read();
        }
    }
}
