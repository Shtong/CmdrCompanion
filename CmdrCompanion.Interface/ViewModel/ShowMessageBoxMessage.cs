using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.ViewModel
{
    public class ShowMessageBoxMessage : GenericMessage<MyMessageBoxViewModel>
    {
        public ShowMessageBoxMessage(MyMessageBoxViewModel content) : base(content)
        {

        }
    }
}
