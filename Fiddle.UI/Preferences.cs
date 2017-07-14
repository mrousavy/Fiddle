using Newtonsoft.Json;
using System;
using System.IO;

namespace Fiddle.UI
{
    public struct Preferences
    {
        public int SelectedLanguage { get; set; }

        public Preferences(int selectedLang)
        {
            SelectedLanguage = selectedLang;
        }
    }

    public class PreferencesManager
    {
        public string PreferencesFile { get; set; }

        public PreferencesManager()
        {
            PreferencesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Fiddle", "Preferences.json");
        }

        public void Load()
        {
            if (!File.Exists(PreferencesFile))
            {
                File.WriteAllText(PreferencesFile, JsonConvert.SerializeObject(new Preferences()));
            }

            string content = File.ReadAllText(PreferencesFile);

        }

        public void WriteOut()
        {

        }
    }
}
