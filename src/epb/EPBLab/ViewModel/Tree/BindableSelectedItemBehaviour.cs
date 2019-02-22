
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace EPBLab.ViewModel.Tree
{
    public class BindableSelectedItemBehaviour : Behavior<TreeView>
    {
        #region SelectedItem Property

        protected ObservableCollection<ITreeNode> _selectedItems = new ObservableCollection<ITreeNode>();
        public ObservableCollection<ITreeNode> SelectedItems
        {
            get => (ObservableCollection<ITreeNode>) GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<ITreeNode>), typeof(BindableSelectedItemBehaviour), new UIPropertyMetadata(null, OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
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
