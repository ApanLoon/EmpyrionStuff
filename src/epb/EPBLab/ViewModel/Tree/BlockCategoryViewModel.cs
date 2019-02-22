using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace EPBLab.ViewModel.Tree
{
    public class BlockCategoryViewModel : ITreeNode
    {
        public string Title { get; set; }

        public string ChildCount
        {
            get => $"({Children.Count})";
        }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public void Add(ITreeNode node)
        {
            Children.Add(node);
        }

        public BlockCategoryViewModel()
        {
            Children = new ObservableCollection<ITreeNode>();
        }
    }
}
