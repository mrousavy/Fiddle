using Fiddle.Compilers;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;

namespace Fiddle.UI {
    public static class Helper {
        public static ICompiler ChangeLanguage(string language, string sourceCode, TextEditor editor) {
            switch (language) {
                case "C#":
                    editor.SyntaxHighlighting = LoadXshd("CSharp.xshd");
                    return NewCompiler(Language.CSharp, sourceCode);
                case "C++":
                    editor.SyntaxHighlighting = LoadXshd("Cpp.xshd");
                    return NewCompiler(Language.Cpp, sourceCode);
                case "VB":
                    editor.SyntaxHighlighting = LoadXshd("VB.xshd");
                    return NewCompiler(Language.Vb, sourceCode);
                case "Python":
                    editor.SyntaxHighlighting = LoadXshd("Python.xshd");
                    return NewCompiler(Language.Python, sourceCode);
                case "Java":
                    editor.SyntaxHighlighting = LoadXshd("Java.xshd");
                    return NewCompiler(Language.Java, sourceCode);
                case "LUA":
                    editor.SyntaxHighlighting = LoadXshd("LUA.xshd");
                    return NewCompiler(Language.Lua, sourceCode);
                default:
                    MessageBox.Show("Language not found!");
                    return null;
            }
        }

        public static ICompiler NewCompiler(Language language, string sourceCode, string[] imports = null) {
            return Host.NewCompiler(language, sourceCode, imports, App.Preferences.JdkPath, App.Preferences.PyPath);
        }


        private static IHighlightingDefinition LoadXshd(string resourceName) {
            Type type = typeof(Helper);
            string fullName = $"{type.Namespace}.Syntax.{resourceName}";
            using (Stream stream = type.Assembly.GetManifestResourceStream(fullName)) {
                if (stream == null)
                    return null;
                using (XmlTextReader reader = new XmlTextReader(stream)) {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }


        public static string SaveFile(string content) {
            SaveFileDialog dialog = new SaveFileDialog {
                Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
                File.WriteAllText(dialog.FileName, content);
            return dialog.FileName;
        }

        public static string SaveFile(string code, Language language) {
            SaveFileDialog dialog = new SaveFileDialog {
                Filter = GetFilterForLanguage(language),
                FilterIndex = 1,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
                File.WriteAllText(dialog.FileName, code);
            return dialog.FileName;
        }

        private static string GetFilterForLanguage(Language language) {
            switch (language) {
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


        public static string ConcatErrors(IEnumerable<Exception> errorsList) {
            string errors = "";
            const int maxErrors = 7; //do not show more than [maxErrors] errors in Message
            int countErrors = 0;

            IEnumerable<Exception> exceptions = errorsList as Exception[] ?? errorsList.ToArray(); //kill multiple enums
            foreach (Exception ex in exceptions) {
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

        public static IEnumerable<Run> BuildDiagnostics(IEnumerable<IDiagnostic> diagnostics, string indent = "") {
            IList<Run> items = new List<Run>();
            int counter = 1;
            string nl = Environment.NewLine;
            foreach (IDiagnostic diagnostic in diagnostics) {
                Brush brush;
                switch (diagnostic.Severity) {
                    case Severity.Error:
                        brush = Brushes.Red;
                        break;
                    case Severity.Warning:
                        brush = Brushes.Yellow;
                        break;
                    default:
                        brush = Brushes.LightGray;
                        break;
                }
                string lines = diagnostic.LineFrom != diagnostic.LineTo
                    ? $"{diagnostic.LineFrom}-{diagnostic.LineTo}"
                    : diagnostic.LineFrom.ToString();

                items.Add(new Run($"{indent}#{counter++} Ln{lines}: ") { Foreground = Brushes.LightGray });
                items.Add(new Run(diagnostic.Message + nl) { Foreground = brush });
            }
            return items;
        }

        public static IEnumerable<Run> BuildRuns(IExecuteResult result) {
            string nl = Environment.NewLine;
            if (result.Success) {
                //Execute: SUCCESS, Compile: SUCCESS
                List<Run> items = new List<Run> {
                    new Run($"Execution successful! (Took {result.Time}ms){nl}") {
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold,
                        FontSize = 15
                    }
                };

                if (result.ReturnValue == null) {
                    //NO RETURN VALUE
                    items.Add(new Run($"Return value: /{nl}") { Foreground = Brushes.Gray });
                } else {
                    //RETURN VALUE(S)
                    Type type = result.ReturnValue.GetType();
                    items.Add(new Run("Return value: "));
                    string typeName;
                    if (type.IsGenericType) {
                        typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}" +
                                   $"<{string.Join(", ", type.GenericTypeArguments.Select(a => a.Name))}>";
                    } else {
                        typeName = type.Name;
                    }
                    items.Add(new Run($"({typeName}) ") { Foreground = Brushes.CadetBlue });
                    if (type.IsArray) {
                        //MULTIPLE RETURN VALUES
                        Array array = (Array)result.ReturnValue;
                        string run = string.Join(", ", array.Cast<object>());
                        items.Add(new Run($"{run}{nl}") {
                            Foreground = Brushes.Orange,
                            FontFamily = new FontFamily("Consolas")
                        });
                    } else if (result.ReturnValue is IList && type.IsGenericType) {
                        //MULTIPLE RETURN VALUES
                        IList list = (IList)result.ReturnValue;
                        string run = string.Join(", ", list.Cast<object>());
                        items.Add(new Run($"{run}{nl}")
                        {
                            Foreground = Brushes.Orange,
                            FontFamily = new FontFamily("Consolas")
                        });
                    } else {
                        //SINGLE RETURN VALUE
                        items.Add(new Run($"{result.ReturnValue}{nl}")
                        {
                            Foreground = Brushes.Orange,
                            FontFamily = new FontFamily("Consolas")
                        });
                    }
                }
                if (string.IsNullOrWhiteSpace(result.ConsoleOutput)) {
                    //NO CONSOLE OUTPUT
                    items.Add(new Run($"Console output: /{nl}") { Foreground = Brushes.Gray });
                } else {
                    //CONSOLE OUTPUT
                    items.Add(new Run("Console output: "));
                    items.Add(new Run(result.ConsoleOutput) {
                        Foreground = Brushes.Orange,
                        FontFamily = new FontFamily("Consolas")
                    });
                }
                if (result.CompileResult.Diagnostics?.Any() == true) {
                    //DIAGNOSTICS
                    items.Add(new Run("Diagnostics:\n"));
                    items.AddRange(BuildDiagnostics(result.CompileResult.Diagnostics, " "));
                }
                return items;
            }

            if (result.CompileResult.Success) {
                //Execute: FAIL, Compile: SUCCESS
                List<Run> items = new List<Run> {
                    new Run($"Execution failed! (Took {result.Time}ms){nl}") {
                        Foreground = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        FontSize = 15
                    }
                };

                if (result.Exception == null) {
                    //NO ERROR MESSAGE
                    items.Add(new Run($"An unexpected error occured.{nl}") { Foreground = Brushes.Gray });
                } else {
                    //ERROR MESSAGE
                    items.Add(new Run($"Ln{result.ExceptionLineNr}: {result.Exception.GetType().Name}: ") { Foreground = Brushes.OrangeRed });
                    items.Add(new Run($"\"{result.Exception.Message}\"{nl}"));
                }
                if (result.CompileResult.Diagnostics?.Any() == true) {
                    //DIAGNOSTICS
                    items.Add(new Run("Diagnostics:\n"));
                    items.AddRange(BuildDiagnostics(result.CompileResult.Diagnostics, " "));
                }

                return items;
            }

            //Execute: FAIL, Compile: FAIL
            return BuildRuns(result.CompileResult);
        }

        public static IEnumerable<Run> BuildRuns(ICompileResult result) {
            string nl = Environment.NewLine;
            if (result.Success) {
                //Compile: SUCCESS
                List<Run> items = new List<Run> {
                    new Run($"Compilation successful! (Took {result.Time}ms){nl}") {
                        Foreground = Brushes.Green,
                        FontWeight = FontWeights.Bold,
                        FontSize = 15
                    }
                };
                if (result.Diagnostics?.Any() == true) {
                    items.Add(new Run("Diagnostics:\n"));
                    items.AddRange(BuildDiagnostics(result.Diagnostics, " "));
                }
                return items;
            } else {
                //Compile: FAIL
                List<Run> items = new List<Run> {
                    new Run($"Compilation failed!{nl}") {
                        Foreground = Brushes.Red,
                        FontWeight = FontWeights.Bold,
                        FontSize = 15
                    }
                };
                if (result.Diagnostics?.Any() == true) {
                    //DIAGNOSTICS
                    items.Add(new Run("Diagnostics:\n"));
                    items.AddRange(BuildDiagnostics(result.Diagnostics, " "));
                }
                return items;
            }
        }
    }
}