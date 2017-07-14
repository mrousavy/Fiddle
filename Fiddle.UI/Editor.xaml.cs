using Fiddle.Compilers;
using MaterialDesignThemes.Wpf;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor
    {
        private ICompiler Compiler { get; set; }

        private string SourceCode => TextBoxCode.Text;

        public Editor()
        {
            InitializeComponent();
            LoadComboBox();
            LoadTextBox();
            TextBoxCode.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetStatus();
        }

        private void LoadComboBox()
        {
            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
        }

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

        private async void ButtonCompile(object sender, RoutedEventArgs e)
        {
            SetStatus(StatusType.Wait, "Compiling..");

            LockUi();
            Compiler.SourceCode = SourceCode;
            ICompileResult result = await Compiler.Compile();
            UnlockUi();

            if (result.Success)
            {
                SetStatus(StatusType.Success, "Compilation was successful!", result.Time);
            }
            else
            {
                SetStatus(StatusType.Failure, "Compilation failed!", result.Time);
                await DialogHelper.ShowErrorDialog($"Compilation failed!\n{result.Errors.First()}", EditorDialogHost);
            }
        }
        private async void ButtonExecute(object sender, RoutedEventArgs e)
        {
            SetStatus(StatusType.Wait, "Executing..");

            LockUi();
            Compiler.SourceCode = SourceCode;
            IExecuteResult result = await Compiler.Execute();
            UnlockUi();
            if (result.Success)
            {
                SetStatus(StatusType.Success, "Execution was successful!", result.CompileResult.Time, result.Time);
            }
            else
            {
                SetStatus(StatusType.Failure, "Execution failed!", result.CompileResult.Time, result.Time);
                await DialogHelper.ShowErrorDialog($"Execution failed!\n{result.Exception.Message}", EditorDialogHost);
            }
        }

        private void ButtonSave(object sender, RoutedEventArgs e)
        {
            LockUi();
            Helper.SaveFile(SourceCode, Compiler.Language);
            UnlockUi();

        }

        private void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e)
        {
            LockUi();
            string value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
            Compiler = Helper.ChangeLanguage(value, SourceCode, TextBoxCode);
            App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;
            Title = $"Fiddle - {value}";
            UnlockUi();
        }
        private void TextBoxCode_KeyDown(object sender, KeyEventArgs e)
        {
            ResetStatus();
        }

        private void LockUi()
        {
            foreach (UIElement element in EditorGrid.Children)
            {
                element.IsEnabled = false;
            }
            Cursor = Cursors.Wait;
        }

        private void UnlockUi()
        {
            foreach (UIElement element in EditorGrid.Children)
            {
                element.IsEnabled = true;
            }
            Cursor = Cursors.Arrow;
            TextBoxCode.Focus();
        }

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
    }
}
