using Fiddle.Compilers;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Windows;
using System.Xml;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Fiddle.UI
{
    public static class Helper
    {

        public static ICompiler ChangeLanguage(string language, string sourceCode, TextEditor editor)
        {
            switch (language)
            {
                case "C#":
                    editor.SyntaxHighlighting = LoadXshd("CSharp.xshd");
                    return Host.NewCompiler(Language.CSharp, sourceCode);
                case "C++":
                    editor.SyntaxHighlighting = LoadXshd("Cpp.xshd");
                    return Host.NewCompiler(Language.Cpp, sourceCode);
                case "VB":
                    editor.SyntaxHighlighting = LoadXshd("Vb.xshd");
                    return Host.NewCompiler(Language.Vb, sourceCode);
                case "Python":
                    editor.SyntaxHighlighting = LoadXshd("Python.xshd");
                    return Host.NewCompiler(Language.Python, sourceCode);
                default:
                    MessageBox.Show("Language not found!");
                    return null;
            }
        }


        private static IHighlightingDefinition LoadXshd(string resourceName)
        {
            Type type = typeof(Helper);
            string fullName = $"{type.Namespace}.Syntax.{resourceName}";
            using (Stream stream = type.Assembly.GetManifestResourceStream(fullName))
            {
                if (stream == null)
                    return null;
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }


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
