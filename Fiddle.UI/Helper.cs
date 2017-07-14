using Microsoft.Win32;
using System.IO;

namespace Fiddle.UI
{
    public static class Helper
    {

        public static void SaveFile(string code)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                if (Directory.Exists(dialog.FileName))
                {

                }
            }
        }
    }
}
