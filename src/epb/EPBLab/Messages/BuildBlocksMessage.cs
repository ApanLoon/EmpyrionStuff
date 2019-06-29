using System.Collections.Generic;
using EPBLab.ViewModel.Tree;
using EPBLib;
using GalaSoft.MvvmLight.Messaging;

namespace EPBLab.Messages
{
    public class BuildBlocksMessage : GenericMessage<IEnumerable<BlockNode>>
    {
        public BuildBlocksMessage(IEnumerable<BlockNode> blockNodes) : base(blockNodes)
        {
            Identifier = string.Empty;
        }

        public string Identifier { get; set; }
    }
}
