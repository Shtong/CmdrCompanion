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
    /// Describes a tradable commodity
    /// </summary>
    public class Commodity : CoreObject
    {
        internal Commodity(string name, string category)
        {
            Name = name;
            Category = category;
            _sellers = new ObservableCollection<AstronomicalObject>();
        }

        private string _name;
        /// <summary>
        /// Gets or sets the commodity name
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
                    throw new ArgumentException("Commodity names cannot be empty", "value");

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _category;
        /// <summary>
        /// Gets or sets the commodity category name. (warning: can be null)
        /// </summary>
        public string Category 
        { 
            get
            {
                return _category;
            }

            set
            {
                _category = value;
                OnPropertyChanged("Category");
            }
        }

        private ObservableCollection<AstronomicalObject> _sellers;
        /// <summary>
        /// Gets a list of stations that sells this commodity
        /// </summary>
        public ObservableCollection<AstronomicalObject> Sellers 
        {
            get
            {
                return _sellers;
            }
        }

        /// <summary>
        /// Returns the string representation of this instance
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return "Commodity " + Name;
        }

        internal void Save(XmlWriter writer)
        {
            writer.WriteStartElement("commodity");
            writer.WriteAttributeString("name", Name);
            if (Category != null)
                writer.WriteAttributeString("category", Category);
            writer.WriteEndElement();
        }

        internal static bool Load(XmlReader reader, EliteEnvironment container)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.LocalName != "commodity")
                return false;

            string cName = null;
            string cCategory = null;
            while(reader.MoveToNextAttribute())
            {
                switch(reader.LocalName)
                {
                    case "name":
                        cName = reader.Value;
                        break;

                    case "category":
                        cCategory = reader.Value;
                        break;
                }
            }

            if(String.IsNullOrWhiteSpace(cName))
                throw new EnvironmentLoadException("Invalid or missing commodity name", reader);

            container.CreateCommodity(cName, cCategory);

            reader.Read();
            return true;
        }
    }
}
