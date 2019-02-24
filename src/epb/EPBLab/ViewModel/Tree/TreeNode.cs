
using System.Collections.ObjectModel;

namespace EPBLab.ViewModel.Tree
{
    public class TreeNode : ITreeNode
    {
        public string Title { get; set; }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public void Add(ITreeNode node)
        {
            Children.Add(node);
        }

        public TreeNode()
        {
            Children = new ObservableCollection<ITreeNode>();
        }
    }
}
