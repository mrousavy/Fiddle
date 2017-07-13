using System;

namespace Fiddle.Compilers.Implementation.VB
{
    public class VbDiagnostic : IDiagnostic
    {
        public string Message { get; }
        public int Line { get; }
        public int Column { get; }
        public int Char { get; }

        public VbDiagnostic(string message, int ln, int col, int ch)
        {
            Message = message;
            Line = ln;
            Column = col;
            Char = ch;
        }

        public Exception ToException()
        {
            return new Exception($"Ln{Line} Col{Column} Ch{Char}: {Message}");
        }
    }
}
