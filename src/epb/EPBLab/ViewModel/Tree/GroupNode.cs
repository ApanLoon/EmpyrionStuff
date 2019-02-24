
namespace EPBLab.ViewModel.Tree
{
    public class GroupNode : TreeNode
    {
        public string ChildCount => $"({Children.Count})";
    }
}
