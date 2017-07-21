using Fiddle.Compilers;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.SharpDevelop.Editor;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Fiddle.UI {
    /// <summary>
    ///     Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor {
        public static RoutedCommand CommandSave = new RoutedCommand(); //Ctrl + S
        public static RoutedCommand CommandCompile = new RoutedCommand(); //F6
        public static RoutedCommand CommandExecute = new RoutedCommand(); //F5
        private ICompiler _compiler; //Compiler instance
        private string _filePath; //path to file - Ctrl + S will save without SaveFileDialog if not null
        private bool _needsUpdate; //need to reset textbox underlines & statusmessage?

        private ITextMarkerService _textMarkerService; //underlines

        //constructor
        public Editor() {
            InitializeComponent();
            LoadComboBox();
            LoadTextBox();
            LoadPreferences();
            LoadTextMarkerService();
            LoadHotkeys();
            TextBoxCode.Focus();
        }

        private string SourceCode => TextBoxCode.Text;

        #region Prefs & Inits

        //Initialize all user-states from preferences
        private void LoadPreferences() {
            //Load last "state"
            if (App.Preferences.SaveUserSettings)
            {
                switch (App.Preferences.StartAction)
                {
                    case StartAction.Continue:
                        TextBoxCode.Text = App.Preferences.SourceCode;
                        TextBoxCode.TextArea.Caret.Offset = App.Preferences.CursorOffset;
                        TextBoxCode.TextArea.Caret.BringCaretToView();
                        break;
                    case StartAction.Specific:
                        TextBoxCode.Text = App.Preferences.DefaultCode;
                        break;
                }

                Width = App.Preferences.WindowWidth;
                Height = App.Preferences.WindowHeight;
                Left = App.Preferences.WindowLeft;
                Top = App.Preferences.WindowTop;
                WindowState = App.Preferences.WindowState;
                GridCodeResults.ColumnDefinitions[2].Width = new GridLength(App.Preferences.ResultsViewSize);
            }
        }

        //load the drop down menu (select saved language)
        private void LoadComboBox() {
            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
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

        //Window loaded event
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ResetStatus();
        }

        //Window closes event
        private void Window_Closing(object sender, CancelEventArgs e) {
            SetStatus(StatusType.Wait, "Closing..");
            if (App.Preferences.SaveUserSettings)
            {
                App.Preferences.WindowWidth = Width;
                App.Preferences.WindowHeight = Height;
                App.Preferences.WindowLeft = Left;
                App.Preferences.WindowTop = Top;
                App.Preferences.WindowState = WindowState;
                App.Preferences.SourceCode = SourceCode;
                App.Preferences.CursorOffset = TextBoxCode.TextArea.Caret.Offset;
                App.Preferences.ResultsViewSize = GridCodeResults.ColumnDefinitions[2].Width.Value;
            }
        }

        #endregion

        #region Code

        //Actually compile code
        private async void Compile() {
            SetStatus(StatusType.Wait, "Compiling..");

            LockUi();
            try {
                _compiler.SourceCode = SourceCode;
                ICompileResult result = await _compiler.Compile();

                SetResultView(result);
                if (result.Success) {
                    SetStatus(StatusType.Success, "Compilation successful!", result.Time);
                } else {
                    SetStatus(StatusType.Failure, "Compilation failed!", result.Time);

                    string errors = Helper.ConcatErrors(result.Errors);
                    await DialogHelper.ShowErrorDialog($"Compilation failed!\n{errors}",
                        EditorDialogHost);
                }
            } catch (Exception ex) {
                SetStatus(StatusType.Failure, "Compilation failed!");
                await DialogHelper.ShowErrorDialog($"Could not compile! ({ex.Message})", EditorDialogHost);
            }
            CodeUnderline();

            UnlockUi();
            _needsUpdate = true;
        }

        //Actually execute code
        private async void Execute() {
            SetStatus(StatusType.Wait, "Executing..");

            LockUi();
            try {
                _compiler.SourceCode = SourceCode;
                IExecuteResult result = await _compiler.Execute();

                SetResultView(result);
                if (result.Success)
                    SetStatus(StatusType.Success, "Execution successful!", result.CompileResult.Time, result.Time);
                else
                    SetStatus(StatusType.Failure, "Execution failed!", result.CompileResult.Time, result.Time);
            } catch (Exception ex) {
                SetStatus(StatusType.Failure, "Execution failed!");
                await DialogHelper.ShowErrorDialog($"Could not execute! ({ex.Message})", EditorDialogHost);
            }
            CodeUnderline();

            UnlockUi();
            _needsUpdate = true;
        }

        //Underline Errors red and warnings/infos Yellow in code
        private void CodeUnderline() {
            ResetUnderline();
            if (_compiler.CompileResult?.Diagnostics == null || !_compiler.CompileResult.Diagnostics.Any())
                return;

            foreach (IDiagnostic diagnostic in _compiler.CompileResult.Diagnostics)
                try {
                    if (diagnostic.LineFrom < 1 || diagnostic.LineTo < 1 || diagnostic.CharFrom < 1 ||
                        diagnostic.CharTo < 1)
                        continue; //invalid diagnostic

                    //LineFrom/LineTo/CharFrom/CharTo -1 because it's 1-based and Lines[] expects an Index
                    int startOffset = TextBoxCode.Document.Lines[diagnostic.LineFrom - 1].Offset + diagnostic.CharFrom -
                                      1;
                    int endOffset = TextBoxCode.Document.Lines[diagnostic.LineTo - 1].Offset + diagnostic.CharTo - 1;
                    int length = endOffset - startOffset;

                    ITextMarker marker = _textMarkerService.Create(startOffset, length);
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    marker.MarkerColor = diagnostic.Severity == Severity.Error ? Colors.Red : Colors.Yellow;
                } catch {
                    // could not underline, out of bounds, etc
                }
        }

        //Remove all Red/Yellow underlinings in code
        private void ResetUnderline() {
            try {
                _textMarkerService?.RemoveAll(m => true);
            } catch {
                //could not reset underline
            }
        }

        //Save the file to disk
        private void ButtonSave(object sender, RoutedEventArgs e) {
            _filePath = null; //Set to null again so save button opens file picker
            Save();
        }

        //Select a different programming language (drop down)
        private async void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e) {
            LockUi();
            ResetUnderline();
            string value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
            try {
                //Try to load the new compiler
                _compiler?.Dispose();
                _compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
                App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;
                Title = $"Fiddle - {value}";
                _filePath = null;
            } catch (Exception ex) {
                await DialogHelper.ShowErrorDialog($"Could not load {value} compiler! ({ex.Message})",
                    EditorDialogHost);
                //Revert changes
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
                value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
                _compiler?.Dispose();
                _compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
            }
            UnlockUi();
        }

        //Text Changed
        private void TextBoxCode_DocumentChanged(object sender, EventArgs e) {
            if (!_needsUpdate) return;

            ResetStatus();
            ResetUnderline();

            _needsUpdate = false;
        }

        #endregion

        #region Helper

        //Freeze every control
        private void LockUi() {
            foreach (UIElement element in EditorGrid.Children) element.IsEnabled = false;
            Cursor = Cursors.Wait;
        }

        //Unfreeze every control
        private void UnlockUi() {
            foreach (UIElement element in EditorGrid.Children) element.IsEnabled = true;
            Cursor = Cursors.Arrow;
            TextBoxCode.Focus();
        }

        //Set Result View content
        private void SetResultView(ICompileResult result) {
            ClearResultView();
            IEnumerable<Run> runs = Helper.BuildRuns(result);
            TextBlockResults.Inlines.AddRange(runs);
        }

        //Set Result View content
        private void SetResultView(IExecuteResult result) {
            ClearResultView();
            IEnumerable<Run> runs = Helper.BuildRuns(result);
            TextBlockResults.Inlines.AddRange(runs);
        }

        //Clear result view
        private void ClearResultView() {
            TextBlockResults.Text = "";
        }

        //Actually save code file
        private void Save() {
            LockUi();
            try {
                if (string.IsNullOrWhiteSpace(_filePath))
                    _filePath = Helper.SaveFile(SourceCode, _compiler.Language);
                else
                    File.WriteAllText(_filePath, SourceCode);

                SetStatus(StatusType.Success, "File saved!");
            } catch (Exception ex) {
                SetStatus(StatusType.Failure, $"Could not save file! ({ex.Message})");
            }
            UnlockUi();
        }

        //Set the status bar's status message/icon
        private void SetStatus(StatusType type, string message = "Ready", long compileTime = -1, long execTime = -1) {
            switch (type) {
                case StatusType.Failure:
                    IconStatus.Kind = PackIconKind.CloseCircleOutline;
                    break;
                case StatusType.Success:
                    IconStatus.Kind = PackIconKind.CheckCircleOutline;
                    break;
                case StatusType.Wait:
                    IconStatus.Kind = PackIconKind.Sync;
                    break;
                default:
                    IconStatus.Kind = PackIconKind.CodeBraces;
                    break;
            }
            TextBlockStatusMessage.Text = message;
            if (compileTime > 0)
                TextBlockCompileTime.Text = $"C: {compileTime}ms";
            if (execTime > 0)
                TextBlockExecuteTime.Text = $"X: {execTime}ms";
        }

        //Reset the status bar's status message/icon
        private void ResetStatus() {
            SetStatus(StatusType.Idle);
        }

        private enum StatusType {
            Idle,
            Success,
            Failure,
            Wait
        }

        #endregion

        #region Events

        //Compile Button click
        private void ButtonCompile(object sender, RoutedEventArgs e) {
            Compile();
        }

        //Execute button click
        private void ButtonExecute(object sender, RoutedEventArgs e) {
            Execute();
        }

        //Ctrl + S Command (Save)
        private void CtrlS(object sender, ExecutedRoutedEventArgs e) {
            Save();
        }

        //F6 Command (Compile)
        private void F6(object sender, ExecutedRoutedEventArgs e) {
            Compile();
        }

        //F5 Command (Execute)
        private void F5(object sender, ExecutedRoutedEventArgs e) {
            Execute();
        }

        #endregion
    }
}