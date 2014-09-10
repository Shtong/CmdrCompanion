using CmdrCompanion.Interface.Modules;
using CmdrCompanion.Interface.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.ViewModel
{
    public class OptionsViewModel : LocalViewModelBase
    {
        public OptionsViewModel()
        {
            CurrentSettings = Settings.Default;
        }

        internal Settings CurrentSettings { get; private set; }

        public bool EmdnEnabled 
        {  
            get
            {
                return CurrentSettings.EmdnEnabled;
            }

            set
            {
                if(value != CurrentSettings.EmdnEnabled)
                {
                    CurrentSettings.EmdnEnabled = value;

                    EmdnUpdater updater = CurrentServiceLocator.GetInstance<EmdnUpdater>();
                    if (value)
                        updater.Enable();
                    else
                        updater.Disable();

                    UpdateMarketdumpState();

                    RaisePropertyChanged("EmdnEnabled");
                }
            }
        }

        public bool UseMarketdumpWithEmdn
        {
            get
            {
                return CurrentSettings.UseMartketdumpWithEmdn;
            }

            set
            {
                if(value != CurrentSettings.UseMartketdumpWithEmdn)
                {
                    CurrentSettings.UseMartketdumpWithEmdn = value;
                    UpdateMarketdumpState();
                    RaisePropertyChanged("UseMarketdumpWithEmdn");
                }
            }
        }

        private void UpdateMarketdumpState()
        {
            MarketDump wrapper = CurrentServiceLocator.GetInstance<MarketDump>();

            bool shouldRun = EmdnEnabled && UseMarketdumpWithEmdn;

            if(wrapper.Running != shouldRun)
            {
                if (shouldRun)
                    wrapper.Start();
                else
                    wrapper.Stop();
            }
        }
    }
}
