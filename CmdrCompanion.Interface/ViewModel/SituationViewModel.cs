using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class SituationViewModel : LocalViewModelBase
    {
        public SituationViewModel()
        {
            ObjectType = new ObjectTypeSelectorViewModel();

            ShowLocationControls = CurrentSituation.CurrentLocation != null;
            ObjectType.Target = CurrentSituation.CurrentLocation;
            CurrentSituation.PropertyChanged += CurrentSituation_PropertyChanged;

        }

        private void CurrentSituation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "CurrentLocation":
                    AstronomicalObject newLoc = CurrentSituation.CurrentLocation;
                    ShowLocationControls = newLoc != null;
                    ObjectType.Target = newLoc;
                    break;
            }
        }

        public Situation CurrentSituation
        {
            get
            {
                return Environment.CurrentSituation;
            }
        }

        private bool _showLocationControls;
        public bool ShowLocationControls 
        {
            get { return _showLocationControls; }
            set
            {
                if(value != _showLocationControls)
                {
                    _showLocationControls = value;
                    RaisePropertyChanged("ShowLocationControls");
                }
            }
        }

        public ObjectTypeSelectorViewModel ObjectType { get; private set; }

        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
