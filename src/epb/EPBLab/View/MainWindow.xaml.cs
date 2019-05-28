using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EPBLab.ViewModel;

namespace EPBLab.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.KeyDownEvent, new KeyEventHandler(TextBox_KeyDown));
            EventManager.RegisterClassHandler(typeof(CheckBox), CheckBox.KeyDownEvent, new KeyEventHandler(CheckBox_KeyDown));
        }


        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.AcceptsReturn == false && e.Key == Key.Enter)
            {
                MoveToNextUIElement(e);
            }
        }

        void CheckBox_KeyDown(object sender, KeyEventArgs e)
        {
            MoveToNextUIElement(e);
            //Sucessfully moved on and marked key as handled.
            //Toggle check box since the key was handled and
            //the checkbox will never receive it.
            if (e.Handled && sender is CheckBox cb)
            {
                cb.IsChecked = !cb.IsChecked;
            }

        }

        void MoveToNextUIElement(KeyEventArgs e)
        {
            // Creating a FocusNavigationDirection object and setting it to a
            // local field that contains the direction selected.
            FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;

            // MoveFocus takes a TraveralReqest as its argument.
            TraversalRequest request = new TraversalRequest(focusDirection);

            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
            {
                if (elementWithFocus.MoveFocus(request)) e.Handled = true;
            }
        }

    }
}