using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.Python {
    public class PyCompileResult : ICompileResult {
        public PyCompileResult(long time, string code,
            IEnumerable<IDiagnostic> diagnostics) {
            Time = time;
            SourceCode = code;


            IEnumerable<IDiagnostic> enumerable = diagnostics as IDiagnostic[] ?? diagnostics.ToArray();
            Diagnostics = enumerable;
            Warnings = enumerable.Where(d => d.Severity == Severity.Warning);
            Errors = enumerable
                .Where(d => d.Severity == Severity.Error)
                .Select(dd => new Exception($"Ln{dd.LineFrom}-{dd.LineTo} Ch{dd.CharFrom}-{dd.CharTo}: {dd.Message}"));
            Success = !Errors.Any();
        }

        public long Time { get; }
        public bool Success { get; }
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings { get; }
        public IEnumerable<Exception> Errors { get; }
    }
}