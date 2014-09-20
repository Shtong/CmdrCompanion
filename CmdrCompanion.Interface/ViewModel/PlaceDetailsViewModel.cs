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
        }

        public void FillWithObject(AstronomicalObject target)
        {
            if(target != Target)
            {
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
    }
}
