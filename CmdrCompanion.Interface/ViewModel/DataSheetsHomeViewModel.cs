using CmdrCompanion.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CmdrCompanion.Interface.ViewModel
{
    public class DataSheetsHomeViewModel : ViewModelBase
    {
        public DataSheetsHomeViewModel()
        {
            EliteEnvironment env = SimpleIoc.Default.GetInstance<EliteEnvironment>();

            CommoditiesView = new ListCollectionView(env.Commodities);
            PlacesView = new ListCollectionView(env.Stars);

            CommoditiesView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            ShowCommodityCommand = new RelayCommand<Commodity>(ShowCommodity);
            ShowStationCommand = new RelayCommand<Station>(ShowStation);
        }

        public ListCollectionView CommoditiesView { get; private set; }

        public ListCollectionView PlacesView { get; private set; }

        public RelayCommand<Commodity> ShowCommodityCommand { get; private set; }

        public void ShowCommodity(Commodity commodity)
        {
            Messenger.Default.Send(new ShowCommodityDetailsMessage(commodity));
        }

        public RelayCommand<Station> ShowStationCommand { get; private set; }

        public void ShowStation(Station station)
        {
            Messenger.Default.Send(new ShowStationDetailsMessage(station));
        }

        #region Messages
        public class ShowStationDetailsMessage : GenericMessage<Station>
        {
            public ShowStationDetailsMessage(Station content) : base (content)
            {

            }
        }

        public class ShowCommodityDetailsMessage : GenericMessage<Commodity>
        {
            public ShowCommodityDetailsMessage(Commodity content) : base(content)
            {

            }
        }
        #endregion
    }
}
