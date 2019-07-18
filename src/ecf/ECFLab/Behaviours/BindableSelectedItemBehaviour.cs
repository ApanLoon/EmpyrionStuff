
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using ECFLab.ViewModel.Tree;

namespace ECFLab.Behaviours
{
    public class BindableSelectedItemBehaviour : Behavior<TreeView>
    {
        #region SelectedItem Property

        protected ObservableCollection<ITreeNode> _selectedItems = new ObservableCollection<ITreeNode>();
        public ObservableCollection<ITreeNode> SelectedItems
        {
            get => (ObservableCollection<ITreeNode>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<ITreeNode>), typeof(BindableSelectedItemBehaviour), new UIPropertyMetadata(null, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is BindableSelectedItemBehaviour behaviour))
            {
                return;
            }
            ObservableCollection<ITreeNode> newSelection = e.NewValue as ObservableCollection<ITreeNode>;
            ITreeNode first = newSelection?.FirstOrDefault();
            if (first == null)
            {
                return;
            }
            TreeView treeView = behaviour.AssociatedObject;
            TreeViewItem item = FindTreeViewItem(treeView, first);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        protected static TreeViewItem FindTreeViewItem(ItemsControl ic, object o)
        {
            if (ic.ItemContainerGenerator.ContainerFromItem(o) is TreeViewItem tvi)
            {
                return tvi;
            }
            foreach (var i in ic.Items)
            {
                if (ic.ItemContainerGenerator.ContainerFromItem(i) is TreeViewItem tvi2)
                {
                    tvi = FindTreeViewItem(tvi2, o);
                    if (tvi != null)
                    {
                        return tvi;
                    }
                }
            }
            return null;
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ITreeNode node)
            {
                ObservableCollection<ITreeNode> newSelection = new ObservableCollection<ITreeNode>
                {
                    node
                };
                this.SelectedItems = newSelection;
            }
        }
    }
}
