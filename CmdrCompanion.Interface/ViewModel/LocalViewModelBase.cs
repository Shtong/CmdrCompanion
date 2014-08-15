using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.ViewModel
{
    public abstract class LocalViewModelBase : ViewModelBase
    {
        private EliteEnvironment _eliteEnvironment;
        public EliteEnvironment Environment
        {
            get
            {
                if(_eliteEnvironment == null)
                {
                    _eliteEnvironment = CurrentServiceLocator.GetInstance<EliteEnvironment>();
                }
                return _eliteEnvironment;
            }
        }

        public IServiceLocator CurrentServiceLocator
        {
            get
            {
                return ServiceLocator.Current;
            }
        }
    }
}
