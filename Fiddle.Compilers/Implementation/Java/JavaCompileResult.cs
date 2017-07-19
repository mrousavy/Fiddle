using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.Java {
    public class JavaCompileResult : ICompileResult {
        public long Time { get; set; }

        public bool Success { get; set; } = true;

        public string SourceCode { get; set; }

        public IEnumerable<IDiagnostic> Diagnostics { get; set; }

        public IEnumerable<IDiagnostic> Warnings { get; set; }

        public IEnumerable<Exception> Errors { get; set; }
        public JavaCompileResult(long time, string code,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors) {
            Time = time;
            SourceCode = code;
            Diagnostics = diagnostics;
            Warnings = warnings;
            Errors = errors;
            if (Errors != null)
                Success = !Errors.Any();
        }
    }
}
