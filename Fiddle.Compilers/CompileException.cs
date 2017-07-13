using System;

namespace Fiddle.Compilers
{
    public class CompileException : Exception
    {
        private const string DefaultMessage = "An unexpected Error occured while trying to compile!";

        public CompileException(Exception exception) : base(DefaultMessage, exception) { }
        public CompileException(string message) : base(message) { }
        public CompileException() : base(DefaultMessage) { }
    }
}
