using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace Fiddle.UI {
    public class Preferences {
        public int SelectedLanguage { get; set; } = -1;

        public double WindowWidth { get; set; } = 1000;
        public double WindowHeight { get; set; } = 700;
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
        public WindowState WindowState { get; set; } = WindowState.Normal;
        public string SourceCode { get; set; } = "";
        public int CursorOffset { get; set; } = 0;
        public double ResultsViewSize { get; set; } = 200;
    }

    public static class PreferencesManager {
        public static string AppData { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiddle");

        public static string PreferencesFile { get; set; } = Path.Combine(AppData, "Preferences.json");

        public static Preferences Load() {
            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);
            if (!File.Exists(PreferencesFile))
                File.WriteAllText(PreferencesFile, JsonConvert.SerializeObject(new Preferences()));

            string content = File.ReadAllText(PreferencesFile);
            return JsonConvert.DeserializeObject<Preferences>(content);
        }

        public static void WriteOut(Preferences prefs) {
            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);
            string content = JsonConvert.SerializeObject(prefs);
            File.WriteAllText(PreferencesFile, content);
        }
    }
}