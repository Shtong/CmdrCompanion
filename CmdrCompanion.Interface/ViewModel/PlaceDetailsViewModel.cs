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
            ObjectType = new ObjectTypeSelectorViewModel();
        }

        public ObjectTypeSelectorViewModel ObjectType { get; private set; }


        public void FillWithObject(AstronomicalObject target)
        {
            if (target != Target)
            {
                ObjectType.Target = target;
                Target = target;
                RaisePropertyChanged("Target");
            }
        }

        public AstronomicalObject Target { get; private set; }

    }
}
