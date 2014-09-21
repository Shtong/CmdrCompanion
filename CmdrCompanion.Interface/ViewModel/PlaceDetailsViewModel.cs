using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class PlaceDetailsViewModel : LocalViewModelBase
    {
        public PlaceDetailsViewModel()
        {
            TypeChoicesList = new List<string>();
            TypeChoicesView = new ListCollectionView(TypeChoicesList);
            TypeChoicesView.CurrentChanged += TypeChoicesView_CurrentChanged;
        }

        void TypeChoicesView_CurrentChanged(object sender, EventArgs e)
        {
            if (TurnOffSaves)
                return;

            string val = TypeChoicesView.CurrentItem as string;
            AstronomicalObject newVersion = null;
            switch(val)
            {
                case "Generic Object":
                    newVersion = AstronomicalObject.CreateAstronomicalObject(Target.Name, Target.Star);
                    break;

                case "Station":
                    newVersion = AstronomicalObject.CreateStation(Target.Name, Target.Star);
                    break;
            }

            if (newVersion != null)
                FillWithObject(newVersion);
        }

        public void FillWithObject(AstronomicalObject target)
        {
            if(target != Target)
            {
                TurnOffSaves = true;
                Target = target;

                if(Target is Star)
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
                    if (Target is Station)
                        TypeChoicesView.MoveCurrentToLast();
                    else
                        TypeChoicesView.MoveCurrentToFirst();
                }

                RaisePropertyChanged("Target");
                TurnOffSaves = false;
            }
        }

        public AstronomicalObject Target { get; private set; }

        private List<string> TypeChoicesList { get; set; }
        public ListCollectionView TypeChoicesView { get; private set; }

        private bool _enableTypeChoice;
        public bool EnableTypeChoice 
        {
            get { return _enableTypeChoice; }
            set
            {
                if(value != _enableTypeChoice)
                {
                    _enableTypeChoice = value;
                    RaisePropertyChanged("EnableTypeChoice");
                }
            }
        }

        private bool TurnOffSaves { get; set; }
    }
}
