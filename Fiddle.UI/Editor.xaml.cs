using Fiddle.Compilers;
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
            LabelStatusMessage.Content = "Ready";
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
            LabelStatusMessage.Content = "Compiling..";

            LockUi();
            Compiler.SourceCode = SourceCode;
            ICompileResult result = await Compiler.Compile();
            UnlockUi();

            LabelCompileTime.Content = $"C: {result.Time}ms";

            if (result.Success)
            {
                LabelStatusMessage.Content = "Compilation successful!";
            }
            else
            {
                LabelStatusMessage.Content = "Compilation failed!";
                await DialogHelper.ShowErrorDialog($"Compilation failed!\n{result.Errors.First()}", EditorDialogHost);
            }
        }
        private async void ButtonExecute(object sender, RoutedEventArgs e)
        {
            LabelStatusMessage.Content = "Executing..";

            LockUi();
            Compiler.SourceCode = SourceCode;
            IExecuteResult result = await Compiler.Execute();
            UnlockUi();

            LabelCompileTime.Content = $"C: {result.CompileResult.Time}ms";
            LabelExecuteTime.Content = $"X: {result.Time}ms";

            if (result.Success)
            {
                LabelStatusMessage.Content = "Execution successful!";
            }
            else
            {
                LabelStatusMessage.Content = "Execution failed!";
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
            LabelExecuteTime.Content = string.Empty;
            LabelCompileTime.Content = string.Empty;
            LabelStatusMessage.Content = "Ready";
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
    }
}
