using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Fiddle.UI.Annotations;

namespace Fiddle.UI {
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        #region MVVM Properties
        private bool _uSettings, _wSize, _wPos, _wState, _lang, _rvSize, _sCode, _cPos;
        private string _jdkPath, _pyPath;
        private long _cTimeout, _eTimeout;

        public bool USettings { //Remember any settings at all
            get => _uSettings;
            set {
                _uSettings = value;
                OnPropertyChanged();
            }
        }
        public bool WSize { //Remember window size
            get => _wSize;
            set {
                _wSize = value;
                OnPropertyChanged();
            }
        }
        public bool WPos { //Remember Window Position
            get => _wPos;
            set {
                _wPos = value;
                OnPropertyChanged();
            }
        }
        public bool WState { //Remember Window State
            get => _wState;
            set {
                _wState = value;
                OnPropertyChanged();
            }
        }
        public bool Lang { //Remember Language
            get => _lang;
            set {
                _lang = value;
                OnPropertyChanged();
            }
        }
        public bool RvSize { //Remember results view size
            get => _rvSize;
            set {
                _rvSize = value;
                OnPropertyChanged();
            }
        }
        public bool SCode { //Remember source code
            get => _sCode;
            set {
                _sCode = value;
                OnPropertyChanged();
            }
        }
        public bool CPos { //Remember cursor position
            get => _cPos;
            set {
                _cPos = value;
                OnPropertyChanged();
            }
        }
        public string JdkPath { //Path to JDK
            get => _jdkPath;
            set {
                _jdkPath = value;
                OnPropertyChanged();
            }
        }
        public string PyPath { //Path to Python library
            get => _pyPath;
            set {
                _pyPath = value;
                OnPropertyChanged();
            }
        }
        public long CTimeout { //Compile timeout
            get => _cTimeout;
            set {
                _cTimeout = value;
                OnPropertyChanged();
            }
        }
        public long ETimeout { //Execute timeout
            get => _eTimeout;
            set {
                _eTimeout = value;
                OnPropertyChanged();
            }
        }
        #endregion


        public Settings() {
            InitializeComponent();
            DataContext = this;
            Load();
        }

        private new bool? DialogResult {
            set {
                try {
                    base.DialogResult = value;
                } catch {
                    //window is not .ShowDialog
                }
            }
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

        //Cancel edit
        private void ButtonCancel(object sender, RoutedEventArgs e) {
            App.Preferences = PreferencesManager.Load();
            try {
                DialogResult = false;
            } catch {
                //window is not .ShowDialog
            }
        }

        //Open %appdata%\Fiddle\preferences.json
        private void ButtonOpenPrefs(object sender, RoutedEventArgs e) {
            Process.Start(PreferencesManager.PreferencesFile);
        }


        private async Task Apply() {
            try {
                App.Preferences.CacheUserSettings = Convert.ToBoolean(USettings);
                App.Preferences.JdkPath = JdkPath;
                App.Preferences.PyPath = PyPath;
                App.Preferences.CompileTimeout = CTimeout;
                App.Preferences.ExecuteTimeout = ETimeout;

                App.Preferences.CacheType = CacheType.Nothing;
                if (WSize)
                    App.Preferences.CacheType |= CacheType.WindowSize;
                if (WPos)
                    App.Preferences.CacheType |= CacheType.WindowPos;
                if (WState)
                    App.Preferences.CacheType |= CacheType.WindowState;
                if (Lang)
                    App.Preferences.CacheType |= CacheType.Language;
                if (RvSize)
                    App.Preferences.CacheType |= CacheType.ResultsViewSize;
                if (SCode)
                    App.Preferences.CacheType |= CacheType.SourceCode;
                if (CPos)
                    App.Preferences.CacheType |= CacheType.CursorPos;
            } catch (Exception ex) {
                //error converting
                await DialogHelper.ShowErrorDialog($"Could not save data! ({ex.Message})", DialogHost);
            }
        }

        private void Load() {
            USettings = App.Preferences.CacheUserSettings;
            JdkPath = App.Preferences.JdkPath;
            PyPath = App.Preferences.PyPath;
            CTimeout = App.Preferences.CompileTimeout;
            ETimeout = App.Preferences.ExecuteTimeout;
            WSize = App.Preferences.CacheType.HasFlag(CacheType.WindowSize);
            WPos = App.Preferences.CacheType.HasFlag(CacheType.WindowPos);
            WState = App.Preferences.CacheType.HasFlag(CacheType.WindowState);
            Lang = App.Preferences.CacheType.HasFlag(CacheType.Language);
            RvSize = App.Preferences.CacheType.HasFlag(CacheType.ResultsViewSize);
            SCode = App.Preferences.CacheType.HasFlag(CacheType.SourceCode);
            CPos = App.Preferences.CacheType.HasFlag(CacheType.CursorPos);
        }


        private static bool IsTextAllowed(string text) {
            Regex regex = new Regex("[^0-9-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void TimeoutTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !IsTextAllowed(e.Text);
        }

        // Use the DataObject.Pasting Handler 
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text)) e.CancelCommand();
            } else {
                e.CancelCommand();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}