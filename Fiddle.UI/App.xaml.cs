using System.Windows;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Preferences Preferences { get; set; }

        public App()
        {
            //Load prefs
            Preferences = PreferencesManager.Load();

            Current.Exit += delegate
            {
                PreferencesManager.WriteOut(Preferences);
            };
        }
    }
}
