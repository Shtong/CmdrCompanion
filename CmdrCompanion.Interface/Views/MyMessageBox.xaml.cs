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
    /// Interaction logic for MyMessageBox.xaml
    /// </summary>
    public partial class MyMessageBox : Window
    {
        public MyMessageBox()
        {
            InitializeComponent();

            Messenger.Default.Register<CloseMessageBoxMessage>(this, OnCloseMessage);
        }

        private void OnCloseMessage(CloseMessageBoxMessage msg)
        {
            if (msg.Content == DataContext)
            {
                Close();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            DragMove();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Messenger.Default.Unregister(this);
        }
    }
}
