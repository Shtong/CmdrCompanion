using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public abstract class CoreObject : NotifyPropertyChanged
    {
        private string _dataSourceName;
        /// <summary>
        /// Gets or sets the name of the data fetcher that modified this instance
        /// </summary>
        public string DataSourceName
        {
            get { return _dataSourceName; }
            set 
            { 
                if(_dataSourceName != value)
                {
                    _dataSourceName = value;
                    OnPropertyChanged(DataSourceName);
                }
            }
        }
    }
}
