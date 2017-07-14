using Fiddle.Compilers;
using System;
using System.IO;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Fiddle.UI
{
    public static class Helper
    {

        public static void SaveFile(string code, Language language)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = GetFilterForLanguage(language),
                FilterIndex = 1,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                File.WriteAllText(dialog.FileName, code);
            }
        }

        private static string GetFilterForLanguage(Language language)
        {
            switch (language)
            {
                case Language.Cpp:
                    return "C++ source files (*.cpp)|*.cpp|All files (*.*)|*.*";
                case Language.CSharp:
                    return "C# source files (*.cs)|*.cs|All files (*.*)|*.*";
                case Language.Python:
                    return "Python source files (*.py)|*.py|All files (*.*)|*.*";
                case Language.Vb:
                    return "Visual Basic source files (*.vb)|*.vb|All files (*.*)|*.*";
                default:
                    return "All files (*.*)|*.*";
            }
        }
    }
}
