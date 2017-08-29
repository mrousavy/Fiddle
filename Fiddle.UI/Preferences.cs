using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Fiddle.Compilers;
using Newtonsoft.Json;

namespace Fiddle.UI {
    [Flags]
    public enum CacheType {
        [Description("Cache nothing")] Nothing = 0,
        [Description("Cache Window size")] WindowSize = 1,
        [Description("Cache Window position")] WindowPos = 2,
        [Description("Cache Window state")] WindowState = 4,
        [Description("Cache Selected Language")] Language = 8,
        [Description("Cache Results View size")] ResultsViewSize = 16,
        [Description("Cache Source Code")] SourceCode = 32,
        [Description("Cache Coursor Position in the sourcecode")] CursorPos = 64
    }

    public class Preferences {
        /// <summary>
        ///     Value indicating if user settings should be saved/loaded (if disabled: all other settings in this file/class are
        ///     ignored (-> performance gain))
        /// </summary>
        public bool CacheUserSettings { get; set; } = true;

        /// <summary>
        ///     The code to load when Preferences.CacheType is Specific
        /// </summary>
        public string DefaultCode { get; set; } = "";

        /// <summary>
        ///     The start action for the source code (<see cref="UI.CacheType.Nothing" /> is most performance)
        /// </summary>
        public CacheType CacheType { get; set; } = CacheType.WindowSize | CacheType.WindowPos | CacheType.WindowState |
                                                   CacheType.Language | CacheType.ResultsViewSize;

        //All settings apply on startup and save on close:
        public Language SelectedLanguage { get; set; } = Language.CSharp; //The selected language in the editor

        public double WindowWidth { get; set; } = 1000; //The editor's window width
        public double WindowHeight { get; set; } = 700; //The editor's window height
        public double WindowLeft { get; set; } = 100; //The editor's window distance to the left corner of the screen
        public double WindowTop { get; set; } = 100; //The editor's window distance to the top corner of the screen

        public string JdkPath { get; set; } = ""; //The path to Java Development Kit
        public string PyPath { get; set; } = ""; //Python Libraries Search Path

        public string[] NetImports { get; set; } = new string[0]; //C#/VB .NET Referenced/Imported Assemblies/Namespaces

        public WindowState WindowState { get; set; } =
            WindowState.Normal; //The editor's window state (minimized, normal, maximized)

        public string SourceCode { get; set; } =
            ""; //The source code in the editor window (performance loss depending on the code length) (only relevant if CacheType.SourceCode)

        public int CursorOffset { get; set; } =
            0; //The Cursor position in the source code (only relevant if CacheType.CursorPos)

        public double ResultsViewSize { get; set; } =
            200; //The size of the Results View window (right side of editor window)

        public long ExecuteTimeout { get; set; } =
            10000; //The maximum amount of time (in ms) code can execute before being terminated (-1 = no timeout)

        public long CompileTimeout { get; set; } =
            10000; //The maximum amount of time (in ms) code can compile before being terminated (-1 = no timeout)
    }

    public static class PreferencesManager {
        public static string AppData { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiddle");

        public static string PreferencesFile { get; set; } = Path.Combine(AppData, "preferences.json");

        /// <summary>
        ///     Load the user preferences from JSON file
        /// </summary>
        /// <returns>Deserialized JSON preferences</returns>
        public static Preferences Load() {
            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);
            if (!File.Exists(PreferencesFile))
                File.WriteAllText(PreferencesFile, JsonConvert.SerializeObject(new Preferences()));

            string content = File.ReadAllText(PreferencesFile);
            return JsonConvert.DeserializeObject<Preferences>(content);
        }

        /// <summary>
        ///     Save the user preferences to JSON file (or don't if <see cref="Preferences.CacheUserSettings" /> is disabled)
        /// </summary>
        /// <param name="prefs">The preferences object to serialize and write out</param>
        public static void WriteOut(Preferences prefs) {
            if (!App.Preferences.CacheUserSettings) //don't save
                return;

            if (!Directory.Exists(AppData))
                Directory.CreateDirectory(AppData);
            string content = JsonConvert.SerializeObject(prefs);
            File.WriteAllText(PreferencesFile, content);
        }
    }
}