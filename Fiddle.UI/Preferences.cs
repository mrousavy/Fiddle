using Newtonsoft.Json;
using System;
using System.IO;

namespace Fiddle.UI
{
    public class Preferences
    {
        public int SelectedLanguage { get; set; } = -1;
    }

    public static class PreferencesManager
    {
        public static string AppData { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiddle");
        public static string PreferencesFile { get; set; } = Path.Combine(AppData, "Preferences.json");

        public static Preferences Load()
        {
            if (!Directory.Exists(AppData))
            {
                Directory.CreateDirectory(AppData);
            }
            if (!File.Exists(PreferencesFile))
            {
                File.WriteAllText(PreferencesFile, JsonConvert.SerializeObject(new Preferences()));
            }

            string content = File.ReadAllText(PreferencesFile);
            return JsonConvert.DeserializeObject<Preferences>(content);
        }

        public static void WriteOut(Preferences prefs)
        {
            if (!Directory.Exists(AppData))
            {
                Directory.CreateDirectory(AppData);
            }
            string content = JsonConvert.SerializeObject(prefs);
            File.WriteAllText(PreferencesFile, content);
        }
    }
}
