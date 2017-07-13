using Fiddle.Compilers;
using System.Windows;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        private ICompiler compiler;
        public Editor()
        {
            InitializeComponent();
            compiler = Host.NewCompiler(Compilers.Language.CSharp, "return \"Hello World!\";");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ICompileResult result = await compiler.Compile();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            IExecuteResult result = await compiler.Execute();
        }
    }
}
