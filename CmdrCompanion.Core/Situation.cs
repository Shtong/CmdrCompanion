using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    public sealed class Situation : CoreObject
    {
        private AstronomicalObject _currentLocation;
        public AstronomicalObject CurrentLocation
        {
            get { return _currentLocation; }
            set
            {
                if(value != _currentLocation)
                {
                    _currentLocation = value;
                    OnPropertyChanged("CurrentLocation");
                }
            }
        }
    }
}
