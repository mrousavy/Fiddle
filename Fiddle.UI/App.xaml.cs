using System.Windows;

namespace Fiddle.UI {
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() {
            //Load prefs
            Preferences = PreferencesManager.Load();

            Current.Exit += delegate { PreferencesManager.WriteOut(Preferences); };
        }

        public static Preferences Preferences { get; set; }
    }
}