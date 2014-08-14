using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Core
{
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
        public string Name 
        { 
            get
            {
                return _name;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Systems cannot have an empty name", "value");

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public StarProximityCollection KnownStarProximities { get; private set; }

        private ObservableCollection<Station> _stations;
        public ReadOnlyObservableCollection<Station> Stations { get; private set; }

        public Station CreateStation(string name)
        {
            Station station = new Station(name, this);
            _stations.Add(station);
            Environment.InternalStations.Add(station);
            return station;
        }

        public Station FindStationByName(string name)
        {
            return _stations.Where(s => s.Name == name).FirstOrDefault();
        }

        public EliteEnvironment Environment { get; private set; }

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

        public override string ToString()
        {
            return "Star system " + Name;
        }
    }
}
