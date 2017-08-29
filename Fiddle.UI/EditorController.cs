using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.SharpDevelop.Editor;

namespace Fiddle.UI {
    /// <summary>
    /// Contains all loading, initialization, preferences and Compiler calls
    /// </summary>
    public partial class Editor {
        #region Prefs & Inits

        //load the drop down menu (select saved language)
        private void LoadComboBox() {
            if (App.Preferences.CacheType.HasFlag(CacheType.Language))
                SelectedLanguage = App.Preferences.SelectedLanguage;
        }

        //load the textbox and disable drag'n'drop for selected text
        private void LoadTextBox() {
            DataObject.AddCopyingHandler(TextBoxCode, (s, e) => {
                if (e.IsDragDrop) e.CancelCommand();
            });
            TextBoxCode.PreviewMouseLeftButtonDown += (s, e) => {
                if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                    TextBoxCode.Select(0, 0);
            };
        }

        //Initialize the custom text marker for underlining
        private void LoadTextMarkerService() {
            TextMarkerService textMarkerService = new TextMarkerService(TextBoxCode.Document);
            TextBoxCode.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            TextBoxCode.TextArea.TextView.LineTransformers.Add(textMarkerService);
            IServiceContainer services =
                (IServiceContainer)TextBoxCode.Document.ServiceProvider.GetService(typeof(IServiceContainer));
            services?.AddService(typeof(ITextMarkerService), textMarkerService);
            _textMarkerService = textMarkerService;
        }

        //Initialize all hotkeys/commands
        private static void LoadHotkeys() {
            CommandSave.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandCompile.InputGestures.Add(new KeyGesture(Key.F6));
            CommandExecute.InputGestures.Add(new KeyGesture(Key.F5));
        }

        //Initialize all user-states from preferences
        private void LoadPreferences() {
            //Load last "state"
            if (App.Preferences.CacheUserSettings) {
                CacheType type = App.Preferences.CacheType;
                if (type == 0)
                    return;
                if (type.HasFlag(CacheType.WindowSize)) {
                    Width = App.Preferences.WindowWidth;
                    Height = App.Preferences.WindowHeight;
                }
                if (type.HasFlag(CacheType.WindowPos)) {
                    Left = App.Preferences.WindowLeft;
                    Top = App.Preferences.WindowTop;
                }
                if (type.HasFlag(CacheType.WindowState)) WindowState = App.Preferences.WindowState;
                if (type.HasFlag(CacheType.ResultsViewSize))
                    GridCodeResults.ColumnDefinitions[2].Width = new GridLength(App.Preferences.ResultsViewSize);
                if (type.HasFlag(CacheType.SourceCode)) TextBoxCode.Text = App.Preferences.SourceCode;
                if (type.HasFlag(CacheType.CursorPos)) {
                    TextBoxCode.TextArea.Caret.Offset = App.Preferences.CursorOffset;
                    TextBoxCode.TextArea.Caret.BringCaretToView();
                }
            }
        }

        #endregion
    }
}
