using System;
using System.IO;
using System.Text;

namespace Fiddle.Compilers.Implementation.CSharp
{
    public class Globals
    {
        public Random Random { get; }
        public StringWriter Console { get; }

        public Globals(StringBuilder builder)
        {
            Console = new StringWriter(builder);
            Random = new Random();
        }
    }
}
