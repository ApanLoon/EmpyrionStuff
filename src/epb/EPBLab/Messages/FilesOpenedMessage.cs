
using System.Collections.Generic;
using System.IO;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.Messages
{
    public class FilesOpenedMessage : GenericMessage<IEnumerable<string>>
    {
        public FilesOpenedMessage(IEnumerable<string> fileNames) : base(fileNames)
        {
            Identifier = string.Empty;
        }

        public string Identifier { get; set; }
    }
}
