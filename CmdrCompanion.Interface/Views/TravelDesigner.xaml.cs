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
    /// Interaction logic for TravelDesigner.xaml
    /// </summary>
    public partial class TravelDesigner : UserControl
    {
        public TravelDesigner()
        {
            InitializeComponent();

            Messenger.Default.Register<TravelDesignerViewModel.ShowDetailedResultsMessage>(this, ShowDetailedResultsHandler);
        }

        private void ShowDetailedResultsHandler(TravelDesignerViewModel.ShowDetailedResultsMessage message)
        {
            TravelDetails window = new TravelDetails();
            window.DataContext = message.Content;
            window.Show();
        }
    }
}
