using Fiddle.Compilers;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
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
                case "Java":
                    editor.SyntaxHighlighting = LoadXshd("Java.xshd");
                    return Host.NewCompiler(Language.Java, sourceCode);
                case "LUA":
                    editor.SyntaxHighlighting = LoadXshd("LUA.xshd");
                    return Host.NewCompiler(Language.Lua, sourceCode);
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
                case Language.Java:
                    return "Java source files (*.java)|*.java|All files (*.*)|*.*";
                case Language.Lua:
                    return "LUA source files (*.lua)|*.lua|All files (*.*)|*.*";
                default:
                    return "All files (*.*)|*.*";
            }
        }


        public static string ConcatErrors(IEnumerable<Exception> errorsList)
        {
            string errors = "";
            const int maxErrors = 7; //do not show more than [maxErrors] errors in Message
            int countErrors = 0;

            IEnumerable<Exception> exceptions = errorsList as Exception[] ?? errorsList.ToArray(); //kill multiple enums
            foreach (Exception ex in exceptions)
            {
                errors += $"#{++countErrors}: {ex.Message}{Environment.NewLine}";

                if (countErrors <= maxErrors) continue;
                //not shown errors (limited to [maxErrors])
                int notShown = exceptions.Count() - countErrors;
                if (notShown > 0)
                    errors += $"{Environment.NewLine}(and {notShown} more..)";
                break;
            }

            return errors;
        }

        public static string ConcatDiagnostics(IEnumerable<IDiagnostic> diagnosticsList, string indent = "")
        {
            int number = 1;
            return diagnosticsList
                .Aggregate("", (current, diagnostic) => current + $"{indent}#{number++}: {diagnostic}{Environment.NewLine}");
        }


        public static IEnumerable<Run> BuildRuns(IExecuteResult result)
        {
            string nl = Environment.NewLine;
            if (result.Success)
            {
                IList<Run> items = new List<Run>
                {
                    new Run($"Execution successful! (Took {result.Time}ms){nl}") { Foreground = Brushes.Green, FontWeight = FontWeights.Bold, FontSize = 15 },
                    !string.IsNullOrWhiteSpace(result.ConsoleOutput)
                        ? new Run($"Console output: {result.ConsoleOutput}{nl}")
                        : new Run($"Console output: /{nl}") { Foreground = Brushes.Gray }
                };

                if (result.ReturnValue == null)
                {
                    items.Add(new Run($"Return value: /{nl}") { Foreground = Brushes.Gray });
                }
                else
                {
                    items.Add(new Run("Return value: "));
                    items.Add(new Run($"({result.ReturnValue.GetType().Name}) ") { Foreground = Brushes.Orange });
                    items.Add(new Run($"{result.ReturnValue}{nl}") { Foreground = Brushes.CadetBlue });
                }
                if (string.IsNullOrWhiteSpace(result.ConsoleOutput))
                {
                    items.Add(new Run($"Console output: /{nl}") { Foreground = Brushes.Gray });
                }
                else
                {
                    items.Add(new Run("Console output: "));
                    items.Add(new Run(result.ConsoleOutput) { Foreground = Brushes.Orange });
                }
                return items;
            }
            else
            {
                if (result.CompileResult.Success)
                {
                    IList<Run> items = new List<Run>
                    {
                        new Run($"Execution failed! (Took {result.Time}ms){nl}")
                        {
                            Foreground = Brushes.Red,
                            FontWeight = FontWeights.Bold,
                            FontSize = 15
                        }
                    };

                    if (result.Exception == null)
                    {
                        items.Add(new Run($"An unexpected error occured.{nl}") { Foreground = Brushes.Gray });
                    }
                    else
                    {
                        items.Add(new Run($"{result.Exception.GetType().Name}: ") { Foreground = Brushes.OrangeRed });
                        items.Add(new Run($"\"{result.Exception.Message}\"{nl}"));
                    }

                    return items;
                }
                else
                {
                    return BuildRuns(result.CompileResult);
                }
            }
        }

        public static IEnumerable<Run> BuildRuns(ICompileResult result)
        {
            string nl = Environment.NewLine;
            if (result.Success)
            {
                string diagnostics = ConcatDiagnostics(result.Diagnostics, " ");
                IList<Run> items = new List<Run>
                {
                    new Run($"Compilation successful! (Took {result.Time}ms){nl}")
                    {
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold,
                        FontSize = 15
                    },
                    new Run(diagnostics) { Foreground = Brushes.LightGray }
                };
                return items;
            }
            else
            {
                string errors = ConcatDiagnostics(result.Diagnostics.Where(d => d.Severity == Severity.Error), " ");
                IList<Run> items = new List<Run>
                {
                    new Run($"Compilation failed!{nl}") {Foreground = Brushes.Red, FontWeight = FontWeights.Bold, FontSize = 15 },
                    new Run(errors) {Foreground = Brushes.LightGray}
                };
                return items;
            }
        }
    }
}
