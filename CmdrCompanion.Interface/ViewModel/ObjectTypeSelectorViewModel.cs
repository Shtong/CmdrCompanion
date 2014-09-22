using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class ObjectTypeSelectorViewModel : LocalViewModelBase
    {
        public ObjectTypeSelectorViewModel(AstronomicalObject target = null)
        {
            TypeChoicesList = new List<string>();
            TypeChoicesView = new ListCollectionView(TypeChoicesList);

            Target = target;

            TypeChoicesView.CurrentChanged += TypeChoicesView_CurrentChanged;
        }

        public AstronomicalObject _target;
        public AstronomicalObject Target 
        {
            get { return _target; }
            set
            {
                if(value != _target)
                {
                    _target = value;

                    TurnOffSaves = true;
                    if(_target == null)
                    {
                        EnableTypeChoice = false;
                        TypeChoicesList.Clear();
                        TypeChoicesView.Refresh();
                    }
                    else if (_target.Type == AstronomicalObjectType.Star)
                    {
                        EnableTypeChoice = false;
                        TypeChoicesList.Clear();
                        TypeChoicesList.Add("Star");
                        TypeChoicesView.Refresh();
                        TypeChoicesView.MoveCurrentToFirst();
                    }
                    else
                    {
                        EnableTypeChoice = true;
                        TypeChoicesList.Clear();
                        TypeChoicesList.Add("Generic Object");
                        TypeChoicesList.Add("Station");
                        TypeChoicesView.Refresh();
                        if (Target.Type == AstronomicalObjectType.Station)
                            TypeChoicesView.MoveCurrentToLast();
                        else
                            TypeChoicesView.MoveCurrentToFirst();
                    }
                    TurnOffSaves = false;

                    RaisePropertyChanged("Target");
                }
            }
        }

        private List<string> TypeChoicesList { get; set; }
        public ListCollectionView TypeChoicesView { get; private set; }

        private bool _enableTypeChoice;
        public bool EnableTypeChoice
        {
            get { return _enableTypeChoice; }
            set
            {
                if (value != _enableTypeChoice)
                {
                    _enableTypeChoice = value;
                    RaisePropertyChanged("EnableTypeChoice");
                }
            }
        }

        private bool TurnOffSaves { get; set; }

        private void TypeChoicesView_CurrentChanged(object sender, EventArgs e)
        {
            if (TurnOffSaves)
                return;

            string val = TypeChoicesView.CurrentItem as string;
            switch (val)
            {
                case "Generic Object":
                    Target.Type = AstronomicalObjectType.NaturalObject;
                    break;

                case "Station":
                    Target.Type = AstronomicalObjectType.Station;
                    break;
            }
        }
    }
}
