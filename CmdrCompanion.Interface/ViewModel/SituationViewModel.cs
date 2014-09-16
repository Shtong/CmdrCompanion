using CmdrCompanion.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class SituationViewModel : LocalViewModelBase
    {
        public Situation CurrentSituation
        {
            get
            {
                return Environment.CurrentSituation;
            }
        }
    }
}
