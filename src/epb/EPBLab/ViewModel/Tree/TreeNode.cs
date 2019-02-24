
using System;
using System.Collections.ObjectModel;
using EPBLab.Helpers;

namespace EPBLab.ViewModel.Tree
{
    public class TreeNode : ITreeNode, IComparable
    {
        public string Title { get; set; }
        public ObservableCollection<ITreeNode> Children { get; set; }

        public void Add(ITreeNode node)
        {
            Children.Add(node);
        }

        public void AddSorted(ITreeNode item)
        {
            Children.AddSorted(item);
        }

        public int CompareTo(object obj)
        {
            if (obj is TreeNode other)
            {
                return String.Compare(Title, other.Title, StringComparison.Ordinal);
            }

            return String.Compare(Title, obj.ToString(), StringComparison.Ordinal);
        }

        public TreeNode()
        {
            Children = new ObservableCollection<ITreeNode>();
        }
    }
}
