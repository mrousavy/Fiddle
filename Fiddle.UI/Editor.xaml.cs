using Fiddle.Compilers;
<<<<<<< HEAD
using MaterialDesignThemes.Wpf;
=======
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.SharpDevelop.Editor;
using System;
using System.ComponentModel.Design;
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Fiddle.UI
{
namespace Fiddle.UI {
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor
    {
    public partial class Editor {
        private ICompiler Compiler { get; set; }
        private string SourceCode => TextBoxCode.Text;

        public Editor()
        {
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

<<<<<<< HEAD
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetStatus();
=======
        private void LoadPreferences() {
            Width = App.Preferences.WindowWidth;
            Height = App.Preferences.WindowHeight;
            Left = App.Preferences.WindowLeft;
            Top = App.Preferences.WindowTop;
            WindowState = App.Preferences.WindowState;
            TextBoxCode.Text = App.Preferences.SourceCode;
            TextBoxCode.TextArea.Caret.Offset = App.Preferences.CursorOffset;
            TextBoxCode.TextArea.Caret.BringCaretToView();
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d
        }

        private void LoadComboBox()
        {
        private void LoadComboBox() {
            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
        }

        private void LoadTextBox()
        {
            DataObject.AddCopyingHandler(TextBoxCode, (s, e) =>
            {
        private void LoadTextBox() {
            DataObject.AddCopyingHandler(TextBoxCode, (s, e) => {
                if (e.IsDragDrop) e.CancelCommand();
            });
            TextBoxCode.PreviewMouseLeftButtonDown += (s, e) =>
            {
            TextBoxCode.PreviewMouseLeftButtonDown += (s, e) => {
                TextBoxCode.Select(0, 0);
            };
        }

<<<<<<< HEAD
        private async void ButtonCompile(object sender, RoutedEventArgs e)
        {
            SetStatus(StatusType.Wait, "Compiling..");
=======
        private void LoadTextMarkerService() {
            TextMarkerService textMarkerService = new TextMarkerService(TextBoxCode.Document);
            TextBoxCode.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            TextBoxCode.TextArea.TextView.LineTransformers.Add(textMarkerService);
            IServiceContainer services = (IServiceContainer)TextBoxCode.Document.ServiceProvider.GetService(typeof(IServiceContainer));
            services?.AddService(typeof(ITextMarkerService), textMarkerService);
            _textMarkerService = textMarkerService;
        }
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LabelStatusMessage.Content = "Ready";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            LockUi();
            Compiler.SourceCode = SourceCode;
            ICompileResult result = await Compiler.Compile();
            UnlockUi();
            App.Preferences.WindowWidth = Width;
            App.Preferences.WindowHeight = Height;
            App.Preferences.WindowLeft = Left;
            App.Preferences.WindowTop = Top;
            App.Preferences.WindowState = WindowState;
            App.Preferences.SourceCode = SourceCode;
            App.Preferences.CursorOffset = TextBoxCode.TextArea.Caret.Offset;
        }

<<<<<<< HEAD
            if (result.Success)
            {
                SetStatus(StatusType.Success, "Compilation was successful!", result.Time);
            }
            else
            {
                SetStatus(StatusType.Failure, "Compilation failed!", result.Time);
                await DialogHelper.ShowErrorDialog($"Compilation failed!\n{result.Errors.First()}", EditorDialogHost);
=======
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
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d
            }
            CodeUnderline();

            UnlockUi();
            NeedsUpdate = true;
        }
<<<<<<< HEAD
        private async void ButtonExecute(object sender, RoutedEventArgs e)
        {
            SetStatus(StatusType.Wait, "Executing..");
=======

        private async void ButtonExecute(object sender, RoutedEventArgs e) {
            LabelStatusMessage.Content = "Executing..";
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d

            LockUi();
            Compiler.SourceCode = SourceCode;
            IExecuteResult result = await Compiler.Execute();
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
<<<<<<< HEAD
            if (result.Success)
            {
                SetStatus(StatusType.Success, "Execution was successful!", result.CompileResult.Time, result.Time);
            }
            else
            {
                SetStatus(StatusType.Failure, "Execution failed!", result.CompileResult.Time, result.Time);
                await DialogHelper.ShowErrorDialog($"Execution failed!\n{result.Exception.Message}", EditorDialogHost);
=======
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
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d
            }
        }

        private void ButtonSave(object sender, RoutedEventArgs e)
        {
        private void ResetUnderline() {
            _textMarkerService.RemoveAll(m => true);
        }

        private void ButtonSave(object sender, RoutedEventArgs e) {
            LockUi();
            Helper.SaveFile(SourceCode, Compiler.Language);
            UnlockUi();

        }

        private void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e)
        {
        private async void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e) {
            LockUi();
            string value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
            Compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
            App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;
            Title = $"Fiddle - {value}";
            try {
                Compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
                App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;
                Title = $"Fiddle - {value}";
            } catch (Exception ex) {
                await DialogHelper.ShowErrorDialog($"Could not load {value} compiler! ({ex.Message})", EditorDialogHost);
            }
            UnlockUi();
        }
<<<<<<< HEAD
        private void TextBoxCode_KeyDown(object sender, KeyEventArgs e)
        {
            ResetStatus();
=======
        private void TextBoxCode_DocumentChanged(object sender, EventArgs e) {
            if (!NeedsUpdate) return;

            LabelExecuteTime.Content = string.Empty;
            LabelCompileTime.Content = string.Empty;
            LabelStatusMessage.Content = "Ready";
            ResetUnderline();

            NeedsUpdate = false;
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d
        }
        #endregion

        private void LockUi()
        {
            foreach (UIElement element in EditorGrid.Children)
            {
        #region Helper
        private void LockUi() {
            foreach (UIElement element in EditorGrid.Children) {
                element.IsEnabled = false;
            }
            Cursor = Cursors.Wait;
        }

        private void UnlockUi()
        {
            foreach (UIElement element in EditorGrid.Children)
            {
        private void UnlockUi() {
            foreach (UIElement element in EditorGrid.Children) {
                element.IsEnabled = true;
            }
            Cursor = Cursors.Arrow;
            TextBoxCode.Focus();
        }
<<<<<<< HEAD

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

        private void ResetStatus() => SetStatus(StatusType.Idle);

        private enum StatusType { Idle, Success, Failure, Wait }
=======
        #endregion
>>>>>>> 2b46bc7045c59c4e2d8a40ef2f9bb83649adf44d
    }
}
