using System.Threading.Tasks;
using Fiddle.UI.Dialogs;
using MaterialDesignThemes.Wpf;

namespace Fiddle.UI {
    public static class DialogHelper {
        public static async Task<bool> ShowYesNoDialog(string question, DialogHost host) {
            CloseDialog(host);
            object result = await host.ShowDialog(new YesNoDialog(question).GetContent());
            return true;
        }

        public static async Task ShowErrorDialog(string message, DialogHost host) {
            CloseDialog(host);
            await host.ShowDialog(new ErrorDialog(message).GetContent());
        }

        public static void CloseDialog(DialogHost host) {
            DialogHost.CloseDialogCommand.Execute(null, host);
        }
    }
}