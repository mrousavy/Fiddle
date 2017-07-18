using Fiddle.Compilers;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.SharpDevelop.Editor;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Fiddle.UI {
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor {
        private ICompiler Compiler { get; set; }
        private string SourceCode => TextBoxCode.Text;

        private ITextMarkerService _textMarkerService;
        private bool NeedsUpdate { get; set; }

        public Editor() {
            InitializeComponent();
            LoadComboBox();
            LoadTextBox();
            LoadPreferences();
            LoadTextMarkerService();
            TextBoxCode.Focus();
        }

        private void LoadPreferences() {
            Width = App.Preferences.WindowWidth;
            Height = App.Preferences.WindowHeight;
            Left = App.Preferences.WindowLeft;
            Top = App.Preferences.WindowTop;
            WindowState = App.Preferences.WindowState;
            TextBoxCode.Text = App.Preferences.SourceCode;
            TextBoxCode.TextArea.Caret.Offset = App.Preferences.CursorOffset;
            TextBoxCode.TextArea.Caret.BringCaretToView();
        }

        private void LoadComboBox() {
            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
        }

        private void LoadTextBox() {
            DataObject.AddCopyingHandler(TextBoxCode, (s, e) => {
                if (e.IsDragDrop) e.CancelCommand();
            });
            TextBoxCode.PreviewMouseLeftButtonDown += (s, e) => {
                TextBoxCode.Select(0, 0);
            };
        }

        private void LoadTextMarkerService() {
            TextMarkerService textMarkerService = new TextMarkerService(TextBoxCode.Document);
            TextBoxCode.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            TextBoxCode.TextArea.TextView.LineTransformers.Add(textMarkerService);
            IServiceContainer services = (IServiceContainer)TextBoxCode.Document.ServiceProvider.GetService(typeof(IServiceContainer));
            services?.AddService(typeof(ITextMarkerService), textMarkerService);
            _textMarkerService = textMarkerService;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LabelStatusMessage.Content = "Ready";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            LockUi();
            App.Preferences.WindowWidth = Width;
            App.Preferences.WindowHeight = Height;
            App.Preferences.WindowLeft = Left;
            App.Preferences.WindowTop = Top;
            App.Preferences.WindowState = WindowState;
            App.Preferences.SourceCode = SourceCode;
            App.Preferences.CursorOffset = TextBoxCode.TextArea.Caret.Offset;
        }

        #region Code
        private async void ButtonCompile(object sender, RoutedEventArgs e) {
            LabelStatusMessage.Content = "Compiling..";

            LockUi();
            try {
                Compiler.SourceCode = SourceCode;
                ICompileResult result = await Compiler.Compile();

                LabelCompileTime.Content = $"C: {result.Time}ms";

                if (result.Success) {
                    LabelStatusMessage.Content = "Compilation successful!";
                } else {
                    LabelStatusMessage.Content = "Compilation failed!";

                    //TODO: Aggregate/Concat to show every exception
                    await DialogHelper.ShowErrorDialog($"Compilation failed!\n{result.Errors.First()}",
                        EditorDialogHost);
                }
            } catch (Exception ex) {
                await DialogHelper.ShowErrorDialog($"Could not compile! ({ex.Message})", EditorDialogHost);
            }
            CodeUnderline();

            UnlockUi();
            NeedsUpdate = true;
        }

        private async void ButtonExecute(object sender, RoutedEventArgs e) {
            LabelStatusMessage.Content = "Executing..";

            LockUi();
            try {
                Compiler.SourceCode = SourceCode;
                IExecuteResult result = await Compiler.Execute();

                LabelCompileTime.Content = $"C: {result.CompileResult.Time}ms";
                LabelExecuteTime.Content = $"X: {result.Time}ms";

                if (result.Success) {
                    LabelStatusMessage.Content = "Execution successful!";
                } else {
                    LabelStatusMessage.Content = "Execution failed!";
                    //TODO: Aggregate/Concat to show every exception
                    await DialogHelper.ShowErrorDialog($"Execution failed!\n{result.Exception.Message}", EditorDialogHost);
                }
            } catch (Exception ex) {
                await DialogHelper.ShowErrorDialog($"Could not execute! ({ex.Message})", EditorDialogHost);
            }
            UnlockUi();
            NeedsUpdate = true;
        }

        private void CodeUnderline() {
            if (Compiler.CompileResult?.Diagnostics == null || !Compiler.CompileResult.Diagnostics.Any())
                return;

            foreach (IDiagnostic diagnostic in Compiler.CompileResult.Diagnostics) {
                int startOffset = TextBoxCode.Document.Lines[diagnostic.LineFrom].Offset + diagnostic.CharFrom;
                int endOffset = TextBoxCode.Document.Lines[diagnostic.LineTo].Offset + diagnostic.CharTo;
                int length = endOffset - startOffset;

                ITextMarker marker = _textMarkerService.Create(startOffset, length);
                marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                marker.MarkerColor = diagnostic.Severity == Severity.Error ? Colors.Red : Colors.Yellow;
            }
        }

        private void ResetUnderline() {
            _textMarkerService.RemoveAll(m => true);
        }

        private void ButtonSave(object sender, RoutedEventArgs e) {
            LockUi();
            Helper.SaveFile(SourceCode, Compiler.Language);
            UnlockUi();
        }

        private async void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e) {
            LockUi();
            string value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
            try {
                Compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
                App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;
                Title = $"Fiddle - {value}";
            } catch (Exception ex) {
                await DialogHelper.ShowErrorDialog($"Could not load {value} compiler! ({ex.Message})", EditorDialogHost);
            }
            UnlockUi();
        }
        private void TextBoxCode_DocumentChanged(object sender, EventArgs e) {
            if (!NeedsUpdate) return;

            LabelExecuteTime.Content = string.Empty;
            LabelCompileTime.Content = string.Empty;
            LabelStatusMessage.Content = "Ready";
            ResetUnderline();

            NeedsUpdate = false;
        }
        #endregion

        #region Helper
        private void LockUi() {
            foreach (UIElement element in EditorGrid.Children) {
                element.IsEnabled = false;
            }
            Cursor = Cursors.Wait;
        }

        private void UnlockUi() {
            foreach (UIElement element in EditorGrid.Children) {
                element.IsEnabled = true;
            }
            Cursor = Cursors.Arrow;
            TextBoxCode.Focus();
        }
        #endregion
    }
}
