using Fiddle.Compilers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        private ICompiler Compiler { get; set; }

        private string SourceCode
        {
            get => new TextRange(TextBoxCode.Document.ContentStart, TextBoxCode.Document.ContentEnd).Text;
            set
            {
                TextBoxCode.Document.Blocks.Clear();
                TextBoxCode.Document.Blocks.Add(new Paragraph(new Run(value)));
            }
        }

        public Editor()
        {
            InitializeComponent();
            LoadComboBox();
        }

        private void LoadComboBox()
        {
            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
        }

        private async void ButtonExecute(object sender, RoutedEventArgs e)
        {
            LockUi();
            IExecuteResult result = await Compiler.Execute();
            UnlockUi();
        }
        private async void ButtonCompile(object sender, RoutedEventArgs e)
        {
            LockUi();
            ICompileResult result = await Compiler.Compile();
            UnlockUi();
        }

        private void ComboBoxLanguageSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            App.Preferences.SelectedLanguage = ComboBoxLanguage.SelectedIndex;

            LockUi();
            string value = ((ComboBoxItem)ComboBoxLanguage.SelectedValue).Content as string;
            switch (value)
            {
                case "C#":
                    Compiler = Host.NewCompiler(Compilers.Language.CSharp, SourceCode);
                    break;
                case "C++":
                    Compiler = Host.NewCompiler(Compilers.Language.Cpp, SourceCode);
                    break;
                case "VB":
                    Compiler = Host.NewCompiler(Compilers.Language.Vb, SourceCode);
                    break;
                case "Python":
                    Compiler = Host.NewCompiler(Compilers.Language.Python, SourceCode);
                    break;
                default:
                    MessageBox.Show("Language not found!");
                    break;
            }
            Title = $"Fiddle - {value}";
            UnlockUi();
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
        }
    }
}
