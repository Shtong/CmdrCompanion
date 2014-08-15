using CmdrCompanion.Interface.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class PluginsViewModel : LocalViewModelBase
    {
        public PluginsViewModel()
        {
            PluginList = ServiceLocator.Current.GetInstance<PluginManager>().AvailablePlugins;
            PluginsView = new ListCollectionView(PluginList);

            TogglePluginCommand = new RelayCommand<PluginContainer>(TogglePlugin);
        }

        public List<PluginContainer> PluginList { get; private set; }

        public ListCollectionView PluginsView { get; private set; }

        public RelayCommand<PluginContainer> TogglePluginCommand { get; private set; }

        public void TogglePlugin(PluginContainer container)
        {
            if (Settings.Default.ActivatedPlugins == null)
                Settings.Default.ActivatedPlugins = new StringCollection();

            if(container.IsActive)
            {
                container.Deactivate();
                Settings.Default.ActivatedPlugins.Remove(container.Name);
            }
            else
            {
                container.Initialize();
                container.Activate();
                Settings.Default.ActivatedPlugins.Add(container.Name);
            }

            // Keep the plugin activated (or not) in the future
            Settings.Default.Save();
        }
    }
}
