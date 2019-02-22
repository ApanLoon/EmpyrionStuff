using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Tree
{
    public interface ITreeNode
    {
        string Title { get; set; }
        ObservableCollection<ITreeNode> Children { get; set; }

    }
}
