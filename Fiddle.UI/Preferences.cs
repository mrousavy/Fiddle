using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Fiddle.UI
{
    public enum StartAction
    {
        [Description("Open a blank page on Fiddle startup")] Blank,
        [Description("Load the last code")] Continue,
        [Description("Load a specific code (Defined in Preferences.DefaultCode)")] Specific
    }

    public class Preferences
    {
        /// <summary>
        /// Value indicating if user settings should be saved/loaded (if disabled: all other settings in this file/class are ignored (-> performance gain))
        /// </summary>
        public bool SaveUserSettings { get; set; } = true;

        /// <summary>
        /// The code to load when Preferences.StartAction is Specific
        /// </summary>
        public string DefaultCode { get; set; } = "";

        /// <summary>
        /// The start action for the source code (Blank is most performance)
        /// </summary>
        public StartAction StartAction { get; set; } = StartAction.Blank;

        //All settings apply on startup and save on close:
        public int SelectedLanguage { get; set; } = -1;  //The selected language in the editor
        public double WindowWidth { get; set; } = 1000; //The editor's window width
        public double WindowHeight { get; set; } = 700; //The editor's window height
        public double WindowLeft { get; set; } = 100; //The editor's window distance to the left corner of the screen
        public double WindowTop { get; set; } = 100; //The editor's window distance to the top corner of the screen
        public WindowState WindowState { get; set; } =
            WindowState.Normal; //The editor's window state (minimized, normal, maximized)
        public string SourceCode { get; set; } =
            ""; //The source code in the editor window (performance loss depending on the code length) (only relevant if StartAction.Continue)
        public int CursorOffset { get; set; } = 0; //The Cursor position in the source code (only relevant if StartAction.Continue)
        public double ResultsViewSize { get; set; } =
            200; //The size of the Results View window (right side of editor window)
    }

    public static class PreferencesManager
    {
        public static string AppData { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiddle");

        public static string PreferencesFile { get; set; } = Path.Combine(AppData, "Preferences.json");

        public static Preferences Load()
        {
            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);
            if (!File.Exists(PreferencesFile))
                File.WriteAllText(PreferencesFile, JsonConvert.SerializeObject(new Preferences()));

            string content = File.ReadAllText(PreferencesFile);
            return JsonConvert.DeserializeObject<Preferences>(content);
        }

        public static void WriteOut(Preferences prefs)
        {
            if (!App.Preferences.SaveUserSettings) //don't save
                return;

            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);
            string content = JsonConvert.SerializeObject(prefs);
            File.WriteAllText(PreferencesFile, content);
        }
    }
}