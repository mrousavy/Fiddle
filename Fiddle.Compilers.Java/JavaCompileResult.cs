using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Java {
    public class JavaCompileResult : ICompileResult {
        public JavaCompileResult(long time, string code,
            IEnumerable<IDiagnostic> diagnostics,
            IEnumerable<IDiagnostic> warnings,
            IEnumerable<Exception> errors) {
            Time = time;
            SourceCode = code;

            Errors = errors ?? new List<Exception>();
            Diagnostics = diagnostics;
            Warnings = warnings;

            Success = !Errors.Any();
        }

        public long Time { get; set; }

        public bool Success { get; set; }

        public string SourceCode { get; set; }

        public IEnumerable<IDiagnostic> Diagnostics { get; set; }

        public IEnumerable<IDiagnostic> Warnings { get; set; }

        public IEnumerable<Exception> Errors { get; set; }
    }
}