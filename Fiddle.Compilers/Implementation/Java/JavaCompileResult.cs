using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.Java {
    public class JavaCompileResult : ICompileResult {
        public JavaCompileResult(long time, string code,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors) {
            Time = time;
            SourceCode = code;

            Errors = errors ?? new List<Exception>();
            IEnumerable<Exception> exceptionsEnumerated = Errors as Exception[] ?? Errors.ToArray();
            Diagnostics = diagnostics ?? exceptionsEnumerated.Select(e => new JavaDiagnostic(e.Message, 0, 0, 0, 0, Severity.Error));
            Warnings = warnings ?? exceptionsEnumerated.Select(e => new JavaDiagnostic(e.Message, 0, 0, 0, 0, Severity.Error));
            Success = !exceptionsEnumerated.Any();
        }

        public long Time { get; set; }

        public bool Success { get; set; }

        public string SourceCode { get; set; }

        public IEnumerable<IDiagnostic> Diagnostics { get; set; }

        public IEnumerable<IDiagnostic> Warnings { get; set; }

        public IEnumerable<Exception> Errors { get; set; }
    }
}