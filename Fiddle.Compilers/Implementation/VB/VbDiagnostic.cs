using Microsoft.Scripting;
using System;

namespace Fiddle.Compilers.Implementation.VB
{
    public class VbDiagnostic : IDiagnostic
    {
        public string Message { get; }
        public int Line { get; }
        public int Char { get; }
        public Severity Severity { get; }

        public VbDiagnostic(string message, int ln, int ch, Severity severity)
        {
            Message = message;
            Line = ln;
            Char = ch;
            Severity = severity;
        }

        public Exception ToException()
        {
            return new Exception($"[{Severity}] Ln{Line} Ch{Char}: {Message}");
        }
    }
}
