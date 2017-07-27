using System;

namespace Fiddle.Compilers.Implementation.LUA {
    public class LuaDiagnostic : IDiagnostic {
        public LuaDiagnostic(string message, int line, Severity severity) {
            Message = message;
            LineFrom = line;
            LineTo = line;
            CharFrom = 1;
            CharTo = 1;
            Severity = severity;
        }

        public string Message { get; }
        public int LineFrom { get; }
        public int LineTo { get; }
        public int CharFrom { get; }
        public int CharTo { get; }
        public Severity Severity { get; }


        public Exception ToException() {
            return new Exception(ToString());
        }

        public override string ToString() {
            return $"[{Severity}] Ln{LineFrom}: {Message}";
        }
    }
}