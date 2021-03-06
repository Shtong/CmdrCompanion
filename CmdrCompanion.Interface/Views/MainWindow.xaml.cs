using CmdrCompanion.Interface.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, e) => ((MainViewModel)DataContext).Start();
            Closing += (sender, e) => ((ViewModelLocator)FindResource("Locator")).Cleanup();

            // Message received to close the application
            Messenger.Default.Register<MainViewModel.CloseMessage>(this, (msg) => Close());

            // Message received to display a modal popup
            Messenger.Default.Register<ShowMessageBoxMessage>(this, (vm) =>
            {
                MyMessageBox popup = new MyMessageBox();
                popup.DataContext = vm.Content;
                popup.ShowDialog();
            });
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if ((e.Source == this || (e.OriginalSource.GetType() == typeof(TabPanel) && e.Source == mainTabs)))
                DragMove();
        }
    }
}
