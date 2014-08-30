using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Base class for core objects that notifies their properties have changed.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        protected void OnPropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new string[] { propertyName });
        }

        protected void OnPropertyChanged(IEnumerable<string> propertyNames = null)
        {
            if (PropertyChanged != null && propertyNames != null)
            {
                foreach (string propName in propertyNames)
                    PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
