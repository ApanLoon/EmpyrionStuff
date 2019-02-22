using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPBLab.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.Messages
{
    public class CloseBlueprintMessage : GenericMessage<BlueprintViewModel>
    {
        public CloseBlueprintMessage(BlueprintViewModel vm) : base(vm)
        {
        }
    }
}
