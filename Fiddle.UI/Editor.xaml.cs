using Fiddle.Compilers;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Fiddle.UI
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        private ICompiler Compiler { get; set; }

        public Editor()
        {
            InitializeComponent();
            FillComboBox();
        }

        private void FillComboBox()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            Language[] values = (Language[])Enum.GetValues(typeof(Language));
            foreach (Language value in values)
            {
                list.Add(value.ToString());
            }
            ComboBoxLanguage.ItemsSource = list;

            if (App.Preferences.SelectedLanguage != -1)
                ComboBoxLanguage.SelectedIndex = App.Preferences.SelectedLanguage;
        }

        private async void ButtonExecute(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IExecuteResult result = await Compiler.Execute();
        }
        private async void ButtonCompile(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ICompileResult result = await Compiler.Compile();
        }

        private void ComboBoxLanguageSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //App.Preferences.SelectedLanguage = e;
        }
    }
}
