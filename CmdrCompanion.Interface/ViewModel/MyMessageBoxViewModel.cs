using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class MyMessageBoxViewModel : LocalViewModelBase
    {
        public MyMessageBoxViewModel()
        {
            OkCommand = new RelayCommand(Ok);
            CloseCommand = new RelayCommand(Close);
        }

        public string Title { get; set; }

        public string MainText { get; set; }

        public RelayCommand OkCommand { get; private set; }

        public void Ok()
        {
            Result = MessageBoxResult.OK;
            MessengerInstance.Send(new CloseMessageBoxMessage(this));
            OnWindowClosed();
        }

        public RelayCommand CloseCommand { get; private set; }

        public void Close()
        {
            Result = MessageBoxResult.Cancel;
            MessengerInstance.Send(new CloseMessageBoxMessage(this));
            OnWindowClosed();
        }

        public MessageBoxResult Result { get; set; }

        public event Action WindowClosed;

        private void OnWindowClosed()
        {
            if (WindowClosed != null)
                WindowClosed();
        }
    }
}
