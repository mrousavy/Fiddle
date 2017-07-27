using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Fiddle.Compilers.Implementation.CSharp {
    public class Globals : IGlobals {
        public Globals(StringBuilder builder) {
            Console = new StringWriter(builder);
            Random = new Random();
            CurrentThread = Thread.CurrentThread;
        }

        public Random Random { get; }
        public StringWriter Console { get; set; }
        public Thread CurrentThread { get; }
    }
}