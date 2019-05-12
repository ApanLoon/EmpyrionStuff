
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.Messages
{
    public class SaveFileSelectedMessage : GenericMessage<string>
    {
        public SaveFileSelectedMessage(string fileName) : base(fileName)
        {
            Identifier = string.Empty;
        }

        public string Identifier { get; set; }
    }
}
