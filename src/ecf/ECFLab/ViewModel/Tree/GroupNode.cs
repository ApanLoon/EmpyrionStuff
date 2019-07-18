using System.Collections.ObjectModel;

namespace ECFLab.ViewModel.Tree
{
    public class GroupNode : ITreeNode
    {
        public string Title { get; set; }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public GroupNode(string title)
        {
            Title = title;
            Children = new ObservableCollection<ITreeNode>();
        }
    }
}
