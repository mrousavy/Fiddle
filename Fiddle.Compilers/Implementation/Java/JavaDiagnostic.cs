using Microsoft.Scripting;
using System;

namespace Fiddle.Compilers.Implementation.Java {
    public class JavaDiagnostic : IDiagnostic {
        public string Message { get; set; }

        public int Line { get; set; }

        public int Char { get; set; }

        public Severity Severity { get; set; }

        public JavaDiagnostic(string message, int ln, int ch, Severity severity) {
            Message = message;
            Line = ln;
            Char = ch;
            Severity = severity;
        }

        public Exception ToException() {
            return new Exception($"[{Severity}] Ln{Line} Ch{Char}: {Message}");
        }
    }
}
