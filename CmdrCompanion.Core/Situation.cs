using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
    /// <summary>
    /// Describes the player's current situation
    /// </summary>
    public sealed class Situation : CoreObject
    {
        private AstronomicalObject _currentLocation;
        /// <summary>
        /// Curretn position of the player. May be <c>null</c> if that position is unknown
        /// </summary>
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
