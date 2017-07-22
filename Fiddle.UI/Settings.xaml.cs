using System;
using System.Windows;

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
        private void ButtonSave(object sender, RoutedEventArgs e) {
            Apply();
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


        private void Apply() {
            try {
                App.Preferences.CacheUserSettings = Convert.ToBoolean(USettings.IsChecked);
                App.Preferences.JdkPath = Jdk.Text;
                App.Preferences.PyPath = PyPaths.Text;

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
            } catch {
                //error converting
            }
        }

        private void Load() {
            USettings.IsChecked = App.Preferences.CacheUserSettings;
            Jdk.Text = App.Preferences.JdkPath;
            Jdk.Text = App.Preferences.JdkPath;
            PyPaths.Text = App.Preferences.PyPath;
            WSize.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.WindowSize);
            WPos.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.WindowPos);
            WState.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.WindowState);
            Lang.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.Language);
            RvSize.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.ResultsViewSize);
            SCode.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.SourceCode);
            CPos.IsChecked = App.Preferences.CacheType.HasFlag(CacheType.CursorPos);
        }
    }
}
