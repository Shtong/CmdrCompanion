using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _sellers = new ObservableCollection<Station>();
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

        private ObservableCollection<Station> _sellers;
        /// <summary>
        /// Gets a list of stations that sells this commodity
        /// </summary>
        public ObservableCollection<Station> Sellers 
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
    }
}
