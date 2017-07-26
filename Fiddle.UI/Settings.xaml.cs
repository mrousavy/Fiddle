using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Fiddle.UI {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings {
        private new bool? DialogResult {
            set {
                try {
                    base.DialogResult = value;
                } catch {
                    //window is not .ShowDialog
                }
            }
        }

        public Settings() {
            InitializeComponent();
            Load();
        }

        //Save Preferences
        private async void ButtonSave(object sender, RoutedEventArgs e) {
            await Apply();
            PreferencesManager.WriteOut(App.Preferences);
            try {
                DialogResult = true;
            } catch {
                //window is not .ShowDialog
            }
        }

        //Open Settings
        private void ButtonCancel(object sender, RoutedEventArgs e) {
            App.Preferences = PreferencesManager.Load();
            try {
                DialogResult = false;
            } catch {
                //window is not .ShowDialog
            }
        }


        private async Task Apply() {
            try {
                App.Preferences.CacheUserSettings = Convert.ToBoolean(USettings.IsChecked);
                App.Preferences.JdkPath = Jdk.Text;
                App.Preferences.PyPath = PyPaths.Text;
                App.Preferences.CompileTimeout = long.Parse(ComTimeout.Text);
                App.Preferences.ExecuteTimeout = long.Parse(ExTimeout.Text);

                App.Preferences.CacheType = CacheType.Nothing;
                if (WSize.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.WindowSize;
                if (WPos.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.WindowPos;
                if (WState.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.WindowState;
                if (Lang.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.Language;
                if (RvSize.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.ResultsViewSize;
                if (SCode.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.SourceCode;
                if (CPos.IsChecked == true)
                    App.Preferences.CacheType |= CacheType.CursorPos;
            } catch(Exception ex) {
                //error converting
                await DialogHelper.ShowErrorDialog($"Could not save data! ({ex.Message})", DialogHost);
            }
        }

        private void Load() {
            USettings.IsChecked = App.Preferences.CacheUserSettings;
            Jdk.Text = App.Preferences.JdkPath;
            Jdk.Text = App.Preferences.JdkPath;
            PyPaths.Text = App.Preferences.PyPath;
            ComTimeout.Text = App.Preferences.CompileTimeout.ToString();
            ExTimeout.Text = App.Preferences.ExecuteTimeout.ToString();
            WSize.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.WindowSize);
            WPos.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.WindowPos);
            WState.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.WindowState);
            Lang.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.Language);
            RvSize.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.ResultsViewSize);
            SCode.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.SourceCode);
            CPos.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.CursorPos);
        }


        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void TimeoutTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        // Use the DataObject.Pasting Handler 
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }
    }
}
