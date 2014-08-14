using CmdrCompanion.Core;
using CmdrCompanion.Interface.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CmdrCompanion.Interface.Views
{
    /// <summary>
    /// Interaction logic for CommodityAnalyzer.xaml
    /// </summary>
    public partial class DataSheetsHome : UserControl
    {
        public DataSheetsHome()
        {
            InitializeComponent();

            Messenger.Default.Register<DataSheetsHomeViewModel.ShowCommodityDetailsMessage>(this, ShowCommodityDetailsHandler);
            Messenger.Default.Register<DataSheetsHomeViewModel.ShowStationDetailsMessage>(this, ShowStationDetailsHandler);
        }

        private void ShowStationDetailsHandler(DataSheetsHomeViewModel.ShowStationDetailsMessage message)
        {

        }

        private void ShowCommodityDetailsHandler(DataSheetsHomeViewModel.ShowCommodityDetailsMessage message)
        {

        }
    }
}
