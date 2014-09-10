using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmdrCompanion.Interface.ViewModel
{
    public sealed class CloseMessageBoxMessage : GenericMessage<MyMessageBoxViewModel>
    {
        public CloseMessageBoxMessage(MyMessageBoxViewModel content) : base(content)
        {

        }
    }
}
