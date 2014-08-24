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
using System.Windows.Shapes;

namespace CmdrCompanion.Interface.Views
{
    /// <summary>
    /// Interaction logic for TravelDetails.xaml
    /// </summary>
    public partial class TravelDetails : Window
    {
        public TravelDetails()
        {
            InitializeComponent();

            Messenger.Default.Register<TravelDetailsViewModel.CloseMessage>(this, OnClose);
        }

        private void OnClose(TravelDetailsViewModel.CloseMessage message)
        {
            Close();
        }
    }
}
