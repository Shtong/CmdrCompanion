using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public class Commodity : CoreObject
    {
        internal Commodity(string name, string category)
        {
            Name = name;
            Category = category;
            _sellers = new ObservableCollection<Station>();
        }

        private string _name;
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
        public ObservableCollection<Station> Sellers 
        {
            get
            {
                return _sellers;
            }
        }

        public override string ToString()
        {
            return "Commodity " + Name;
        }
    }
}
