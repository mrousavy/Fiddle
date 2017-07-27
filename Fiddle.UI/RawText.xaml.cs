using System.Windows;

namespace Fiddle.UI {
    /// <summary>
    ///     Interaction logic for RawText.xaml
    /// </summary>
    public partial class RawText  {
        public RawText(string text) {
            InitializeComponent();
            Text.Text = text;
        }

        private async void ButtonClose(object sender, RoutedEventArgs e) {
            await this.AnimateAsync(OpacityProperty, 1, 0, 200);
            Close();
        }

        private async void ButtonSave(object sender, RoutedEventArgs e) {
            string filename = Helper.SaveFile(Text.Text);
            if (string.IsNullOrWhiteSpace(filename))
                return;
            
            await this.AnimateAsync(OpacityProperty, 1, 0, 200);
            Close();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            this.Animate(OpacityProperty, 0, 1, 200);
        }
    }
}