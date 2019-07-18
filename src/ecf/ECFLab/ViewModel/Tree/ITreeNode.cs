using System.Collections.ObjectModel;

namespace ECFLab.ViewModel.Tree
{
    public interface ITreeNode
    {
        string Title { get; set; }
        ObservableCollection<ITreeNode> Children { get; set; }
    }
}