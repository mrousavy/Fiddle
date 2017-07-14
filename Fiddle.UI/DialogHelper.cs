using Fiddle.UI.Dialogs;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;

namespace Fiddle.UI
{
    public static class DialogHelper
    {
        public static async Task<bool> ShowYesNoDialog(string question, DialogHost host)
        {
            object result = await host.ShowDialog(new YesNoDialog(question).GetContent());
            return true;
        }

        public static async Task<bool> ShowErrorDialog(string message, DialogHost host)
        {
            object result = await host.ShowDialog(new ErrorDialog(message).GetContent());
            return true;
        }
    }
}
