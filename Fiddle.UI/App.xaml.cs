using System.Windows;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Preferences Preferences { get; private set; }

        public App()
        {
            //Load prefs
            Preferences = new Preferences();
        }
    }
}
