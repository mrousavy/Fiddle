using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Fiddle.UI {
    /// <summary>
    ///     Interaction logic for RawText.xaml
    /// </summary>
    public partial class RawText : Window {
        public RawText(string text) {
            InitializeComponent();
            Text.Text = text;
        }

        private async void ButtonClose(object sender, RoutedEventArgs e) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += delegate { tcs.SetResult(true); };
            BeginAnimation(OpacityProperty, fadeOut);

            await tcs.Task;

            Close();
        }

        private async void ButtonSave(object sender, RoutedEventArgs e) {
            string filename = Helper.SaveFile(Text.Text);
            if (string.IsNullOrWhiteSpace(filename))
                return;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += delegate { tcs.SetResult(true); };
            BeginAnimation(OpacityProperty, fadeOut);

            await tcs.Task;

            Close();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            BeginAnimation(OpacityProperty, fadeIn);
        }
    }
}