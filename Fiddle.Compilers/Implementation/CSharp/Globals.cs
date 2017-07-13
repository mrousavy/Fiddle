using System;
using System.IO;

namespace Fiddle.Compilers.Implementation.CSharp
{
    public class Globals
    {
        public Random Random { get; } = new Random();
        public TextWriter Console { get; } = new StreamWriter(new MemoryStream());
    }
}
