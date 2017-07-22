using System;
using System.Windows;

namespace Fiddle.UI {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings {
        private Preferences Prefs { get; set; }
        private new bool? DialogResult {
            set {
                try {
                    base.DialogResult = value;
                } catch {
                    //window is not .ShowDialog
                }
            }
        }

        public Settings(Preferences prefs) {
            InitializeComponent();
            Prefs = prefs;
            Load();
        }


        //Save Preferences
        private void ButtonSave(object sender, RoutedEventArgs e) {
            Apply();
            PreferencesManager.WriteOut(Prefs);
            try {
                DialogResult = true;
            } catch {
                //window is not .ShowDialog
            }
        }

        //Open Settings
        private void ButtonCancel(object sender, RoutedEventArgs e) {
            Prefs = PreferencesManager.Load();
            try {
                DialogResult = false;
            } catch {
                //window is not .ShowDialog
            }
        }


        private void Apply() {
            try {
                Prefs.CacheUserSettings = Convert.ToBoolean(USettings.IsChecked);
                Prefs.CacheType = CacheType.Nothing;
                if (WSize.IsChecked == true)
                    Prefs.CacheType |= CacheType.WindowSize;
                if (WPos.IsChecked == true)
                    Prefs.CacheType |= CacheType.WindowPos;
                if (WState.IsChecked == true)
                    Prefs.CacheType |= CacheType.WindowState;
                if (Lang.IsChecked == true)
                    Prefs.CacheType |= CacheType.Language;
                if (RvSize.IsChecked == true)
                    Prefs.CacheType |= CacheType.ResultsViewSize;
                if (SCode.IsChecked == true)
                    Prefs.CacheType |= CacheType.SourceCode;
                if (CPos.IsChecked == true)
                    Prefs.CacheType |= CacheType.CursorPos;
            } catch {
                //error converting
            }
        }

        private void Load() {
            USettings.IsChecked = Prefs.CacheUserSettings;
            WSize.IsChecked = Prefs.CacheType.HasFlag(CacheType.WindowSize);
            WPos.IsChecked = Prefs.CacheType.HasFlag(CacheType.WindowPos);
            WState.IsChecked = Prefs.CacheType.HasFlag(CacheType.WindowState);
            Lang.IsChecked = Prefs.CacheType.HasFlag(CacheType.Language);
            RvSize.IsChecked = Prefs.CacheType.HasFlag(CacheType.ResultsViewSize);
            SCode.IsChecked = Prefs.CacheType.HasFlag(CacheType.SourceCode);
            CPos.IsChecked = Prefs.CacheType.HasFlag(CacheType.CursorPos);
        }
    }
}
