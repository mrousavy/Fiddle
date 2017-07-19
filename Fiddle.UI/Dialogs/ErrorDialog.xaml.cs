using System.Windows.Controls;

namespace Fiddle.UI.Dialogs {
    /// <summary>
    ///     Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Page {
        public ErrorDialog(string text) {
            InitializeComponent();
            LabelText.Content = text;
        }

        public Grid GetContent() {
            return MainGrid;
        }
    }
}