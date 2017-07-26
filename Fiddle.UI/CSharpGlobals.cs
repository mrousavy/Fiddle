using System;
using System.IO;
using System.Text;
using System.Windows;
using Fiddle.Compilers;

namespace Fiddle.UI
{
    public class CSharpGlobals : IGlobals {
        public CSharpGlobals(StringBuilder builder, Editor caller) {
            Console = new StringWriter(builder);
            Editor = caller;
            App = Application.Current;
            Random = new Random();
        }

        public Random Random { get; }
        public StringWriter Console { get; }
        public Editor Editor { get; set; }
        public Application App { get; set; }
    }
}
