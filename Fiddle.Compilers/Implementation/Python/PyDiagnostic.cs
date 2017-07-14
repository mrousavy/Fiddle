using System;

namespace Fiddle.Compilers.Implementation.Python
{
    public class PyDiagnostic : IDiagnostic
    {
        public string Message { get; }
        public int Line { get; }
        public int Char { get; }


        public PyDiagnostic(string message, int line, int ch)
        {
            Message = message;
            Line = line;
            Char = ch;
        }

        public Exception ToException()
        {
            return new Exception($"Ln{Line} Ch{Char}: {Message}");
        }
    }
}
