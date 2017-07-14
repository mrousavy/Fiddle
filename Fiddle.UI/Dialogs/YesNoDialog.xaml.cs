using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace Fiddle.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for YesNoDialog.xaml
    /// </summary>
    public partial class YesNoDialog : Page
    {
        public YesNoDialog(string text)
        {
            InitializeComponent();
            LabelText.Content = text;
        }

        public Grid GetContent()
        {
            return MainGrid;
        }

        private void ButtonNo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, this);
        }

        private void ButtonYes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, this);
        }
    }
}
