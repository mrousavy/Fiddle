using Fiddle.Compilers;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.SharpDevelop.Editor;
using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor
    {
        //TODO: Compile/Execute Result View

        private string SourceCode => TextBoxCode.Text;

        private ITextMarkerService _textMarkerService;
        private bool _needsUpdate;
        private ICompiler _compiler;

        //constructor
        public Editor()
        {
            InitializeComponent();
            LoadComboBox();
            LoadTextBox();
            LoadPreferences();
            LoadTextMarkerService();
            TextBoxCode.Focus();
        }

        #region Prefs & Inits
        //Initialize all user-states from preferences
        private void LoadPreferences()
        {
            //Load last "state"
            Width = App.Preferences.WindowWidth;
            Height = App.Preferences.WindowHeight;
            Left = App.Preferences.WindowLeft;
            Top = App.Preferences.WindowTop;
            WindowState = App.Preferences.WindowState;
            TextBoxCode.Text = App.Preferences.SourceCode;
            TextBoxCode.TextArea.Caret.Offset = App.Preferences.CursorOffset;
            TextBoxCode.TextArea.Caret.BringCaretToView();
        }
        //load the drop down menu (select saved language)
        private void LoadComboBox()
        {
            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
        }
        //load the textbox and disable drag'n'drop
        private void LoadTextBox()
        {
            DataObject.AddCopyingHandler(TextBoxCode, (s, e) =>
            {
                if (e.IsDragDrop) e.CancelCommand();
            });
            TextBoxCode.PreviewMouseLeftButtonDown += (s, e) =>
            {
                TextBoxCode.Select(0, 0);
            };
        }
        //Initialize the custom text marker for underlining
        private void LoadTextMarkerService()
        {
            TextMarkerService textMarkerService = new TextMarkerService(TextBoxCode.Document);
            TextBoxCode.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            TextBoxCode.TextArea.TextView.LineTransformers.Add(textMarkerService);
            IServiceContainer services = (IServiceContainer)TextBoxCode.Document.ServiceProvider.GetService(typeof(IServiceContainer));
            services?.AddService(typeof(ITextMarkerService), textMarkerService);
            _textMarkerService = textMarkerService;
        }
        //Window loaded event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetStatus();
        }
        //Window closes event
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetStatus(StatusType.Wait, "Closing..");
            App.Preferences.WindowWidth = Width;
            App.Preferences.WindowHeight = Height;
            App.Preferences.WindowLeft = Left;
            App.Preferences.WindowTop = Top;
            App.Preferences.WindowState = WindowState;
            App.Preferences.SourceCode = SourceCode;
            App.Preferences.CursorOffset = TextBoxCode.TextArea.Caret.Offset;
        }
        #endregion

        #region Code
        //Compile Button click
        private async void ButtonCompile(object sender, RoutedEventArgs e)
        {
            SetStatus(StatusType.Wait, "Compiling..");

            LockUi();
            try
            {
                _compiler.SourceCode = SourceCode;
                ICompileResult result = await _compiler.Compile();

                if (result.Success)
                {
                    SetStatus(StatusType.Success, "Compilation successful!", result.Time);
                }
                else
                {
                    SetStatus(StatusType.Failure, "Compilation failed!", result.Time);

                    string errors = Helper.ConcatErrors(result.Errors);
                    await DialogHelper.ShowErrorDialog($"Compilation failed!\n{errors}",
                    EditorDialogHost);
                }
            }
            catch (Exception ex)
            {
                SetStatus(StatusType.Failure, "Compilation failed!");
                await DialogHelper.ShowErrorDialog($"Could not compile! ({ex.Message})", EditorDialogHost);
            }
            CodeUnderline();

            UnlockUi();
            _needsUpdate = true;
        }

        //Execute button click
        private async void ButtonExecute(object sender, RoutedEventArgs e)
        {
            SetStatus(StatusType.Wait, "Executing..");

            LockUi();
            try
            {
                _compiler.SourceCode = SourceCode;
                IExecuteResult result = await _compiler.Execute();

                if (result.Success)
                {
                    SetStatus(StatusType.Success, "Execution successful!", result.CompileResult.Time, result.Time);
                }
                else
                {
                    SetStatus(StatusType.Failure, "Execution failed!", result.CompileResult.Time, result.Time);
                    await DialogHelper.ShowErrorDialog($"Execution failed!\n{result.Exception.Message}", EditorDialogHost);
                }
            }
            catch (Exception ex)
            {
                SetStatus(StatusType.Failure, "Execution failed!");
                await DialogHelper.ShowErrorDialog($"Could not execute! ({ex.Message})", EditorDialogHost);
            }
            UnlockUi();
            _needsUpdate = true;
        }

        //Underline Errors red and warnings/infos Yellow in code
        private void CodeUnderline()
        {
            if (_compiler.CompileResult?.Diagnostics == null || !_compiler.CompileResult.Diagnostics.Any())
                return;

            foreach (IDiagnostic diagnostic in _compiler.CompileResult.Diagnostics)
            {
                int startOffset = TextBoxCode.Document.Lines[diagnostic.LineFrom].Offset + diagnostic.CharFrom;
                int endOffset = TextBoxCode.Document.Lines[diagnostic.LineTo].Offset + diagnostic.CharTo;
                int length = endOffset - startOffset;

                ITextMarker marker = _textMarkerService.Create(startOffset, length);
                marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                marker.MarkerColor = diagnostic.Severity == Severity.Error ? Colors.Red : Colors.Yellow;
            }
        }
        //Remove all Red/Yellow underlinings in code
        private void ResetUnderline()
        {
            _textMarkerService.RemoveAll(m => true);
        }
        //Save the file to disk
        private void ButtonSave(object sender, RoutedEventArgs e)
        {
            LockUi();
            Helper.SaveFile(SourceCode, _compiler.Language);
            UnlockUi();
        }
        //Select a different programming language (drop down)
        private async void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e)
        {
            LockUi();
            string value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
            try
            {
                _compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
                App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;
                Title = $"Fiddle - {value}";
            }
            catch (Exception ex)
            {
                await DialogHelper.ShowErrorDialog($"Could not load {value} compiler! ({ex.Message})", EditorDialogHost);
            }
            UnlockUi();
        }
        //Text Changed
        private void TextBoxCode_DocumentChanged(object sender, EventArgs e)
        {
            if (!_needsUpdate) return;

            ResetStatus();
            ResetUnderline();

            _needsUpdate = false;
        }
        #endregion

        #region Helper
        //Freeze every control
        private void LockUi()
        {
            foreach (UIElement element in EditorGrid.Children)
            {
                element.IsEnabled = false;
            }
            Cursor = Cursors.Wait;
        }
        //Unfreeze every control
        private void UnlockUi()
        {
            foreach (UIElement element in EditorGrid.Children)
            {
                element.IsEnabled = true;
            }
            Cursor = Cursors.Arrow;
            TextBoxCode.Focus();
        }

        //Set the status bar's status message/icon
        private void SetStatus(StatusType type, string message = "Ready", long compileTime = -1, long execTime = -1)
        {
            switch (type)
            {
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
            LabelStatusMessage.Content = message;
            if (compileTime > 0)
                LabelCompileTime.Content = $"C: {compileTime}ms";
            if (execTime > 0)
                LabelExecuteTime.Content = $"X: {execTime}ms";
        }
        //Reset the status bar's status message/icon
        private void ResetStatus() => SetStatus(StatusType.Idle);

        private enum StatusType { Idle, Success, Failure, Wait }
        #endregion
    }
}
