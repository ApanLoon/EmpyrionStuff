using System.Collections;
using System.Collections.ObjectModel;
using ECFLab.ViewModel.Tree;
using ECFLib;

namespace ECFLab.ViewModel.Blocks
{
    public class BlockViewModel : ITreeNode
    {
        public BlockType BlockType { get; set; }
        public string Title { get; set; }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public BlockViewModel Parent { get; set; }

        public int Id => BlockType.Id;
        public string Name => BlockType.Name;
        public string RefName => BlockType.RefName;
        public IEnumerable Attributes => BlockType.Attributes.Values;

        public BlockViewModel(BlockType blockType)
        {
            BlockType = blockType;
            Title = blockType.Name;
            Children = new ObservableCollection<ITreeNode>();
        }
    }
}
