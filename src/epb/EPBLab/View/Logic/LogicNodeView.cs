using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using EPBLab.ViewModel.Logic;

namespace EPBLab.View.Logic
{
    public class LogicNodeView : UserControl
    {
        protected LogicNodeView()
        {
            LayoutUpdated += OnLayoutUpdated;
            SizeChanged += OnSizeChanged;
        }

        /// <summary>
        /// Automatically updated dependency property that specifies the centre point of the view.
        /// These coordinates are relative to the parent control specified by the 'Ancestor' property.
        /// </summary>
        public Point Centre
        {
            get
            {
                return (Point)GetValue(CentreProperty);
            }
            set
            {
                SetValue(CentreProperty, value);
            }
        }
        public static readonly DependencyProperty CentreProperty = DependencyProperty.Register("Centre", typeof(Point), typeof(LogicNodeView));


        public FrameworkElement Ancestor
        {
            get
            {
                return (FrameworkElement)GetValue(AncestorProperty);
            }
            set
            {
                SetValue(AncestorProperty, value);
            }
        }
        public static readonly DependencyProperty AncestorProperty = DependencyProperty.Register("Ancestor", typeof(FrameworkElement), typeof(LogicNodeView), new FrameworkPropertyMetadata(Ancestor_PropertyChanged));


        public Size Size
        {
            get
            {
                return (Size)GetValue(SizeProperty);
            }
            set
            {
                SetValue(SizeProperty, value);
            }
        }
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(Size), typeof(LogicNodeView));




        /// <summary>
        /// Event raised when 'Ancestor' property has changed.
        /// </summary>
        private static void Ancestor_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LogicNodeView c = (LogicNodeView)d;
            c.UpdateCentre();
        }

        /// <summary>
        /// Event raised when the layout of the connector has been updated.
        /// </summary>
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            UpdateCentre();
        }

        /// <summary>
        /// Event raised when the size of the connector has changed.
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Size = e.NewSize;
            UpdateCentre();
        }

        private void UpdateCentre()
        {
            if (this.Ancestor == null || !Ancestor.IsVisible || !this.IsVisible)
            {
                return;
            }

            // Calculate the center point (in local coordinates) of the connector.
            var center = new Point(this.ActualWidth / 2, this.ActualHeight / 2);

            // Transform the local center point so that it is the center of the connector relative
            // to the specified ancestor.
            var centerRelativeToAncestor = this.TransformToAncestor(this.Ancestor).Transform(center);

            // Assign the computed point to the 'Centre' property.  Data-binding will take care of 
            // pushing this value into the data-model.
            this.Centre = centerRelativeToAncestor;
        }




        protected void DragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (thumb == null)
                return;

            var node = thumb.DataContext as LogicNodeViewModel;
            if (node == null)
                return;

            node.X += e.HorizontalChange; // TODO: Can we make this happen via binding like Centre instead of assuming the viewmodel?
            node.Y += e.VerticalChange;
        }

    }
}
