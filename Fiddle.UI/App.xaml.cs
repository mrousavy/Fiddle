using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Fiddle.UI
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App 
    {
        public App()
        {
            //Load prefs
            Preferences = PreferencesManager.Load();

            DispatcherUnhandledException += UnhandledException;

            Current.Exit += delegate { PreferencesManager.WriteOut(Preferences); };
        }

        private static void UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string nl = Environment.NewLine;
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fiddle", "error.txt");
                string content = BuildBody(e.Exception);
                File.WriteAllText(path, content);
                MessageBoxResult result = MessageBox.Show($"An unknown error occured in Fiddle.{nl}{nl}" +
                                $"An error report has been saved to \"{path}\".{nl}" +
                                $"You can help by submitting the report.{nl}" +
                                "Do you want to go there now?",
                    "Fiddle - Unexpected Error",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("http://github.com/mrousavy/Fiddle/issues/new");
                    Process.Start("explorer.exe", $"/select, \"{path}\"");
                }
                Process.GetCurrentProcess().Kill();
            }
            catch
            {
                // all hope is lost
            }
        }

        private static string BuildBody(Exception ex)
        {
            string nl = Environment.NewLine;
            const string header1 = "THIS ERROR REPORT FILE WAS AUTOMATICALLY CREATED BY FIDDLE";
            const string header2 = "PLEASE SUBMIT THIS FILE AT: http://github.com/mrousavy/Fiddle/issues/new";
            int length = Math.Max(header1.Length, header2.Length);
            string exDetails = GetExceptionDetails(ex);

            string content = $"{header1}{nl}{header2}{nl}{new string('-', length)}{nl}{nl}" +
                             $"BEGIN EXCEPTION DETAILS:{nl}{nl}{exDetails}";
            return content;
        }

        private static string GetExceptionDetails(Exception ex, string indent = "")
        {
            string nl = Environment.NewLine;
            string details = string.Empty;
            indent += "\t";
            if (ex.InnerException != default(Exception))
                details = GetExceptionDetails(ex.InnerException, indent);

            string indentedTrace = ex.StackTrace.Replace(nl, $"{indent}{new string(' ', 14)}{nl}");

            details += $"{indent}{ex.GetType()}:{nl}" +
                       $"{indent}  Message: \"{ex.Message}\"{nl}" +
                       $"{indent}  StackTrace: {indentedTrace}{nl}{nl}";

            return details;
        }

        public static Preferences Preferences { get; set; }
    }
}