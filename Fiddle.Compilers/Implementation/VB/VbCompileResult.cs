using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Fiddle.Compilers.Implementation.VB
{
    public class VbCompileResult : ICompileResult
    {
        public long Time { get; }
        public bool Success { get; } = true;
        public string SourceCode { get; }
        public IEnumerable<IDiagnostic> Diagnostics { get; }
        public IEnumerable<IDiagnostic> Warnings => Diagnostics;
        public IEnumerable<Exception> Errors => Warnings.Select(d => new Exception(d.Message));


        public VbCompileResult(long time, string code, CompilerErrorCollection errors)
        {
            Time = time;
            SourceCode = code;

            Diagnostics = new List<IDiagnostic>();
            foreach (CompilerError error in errors)
            {
                ((List<IDiagnostic>)Diagnostics).Add(
                    new VbDiagnostic(
                        error.ErrorText,
                        error.Line,
                        error.Line,
                        error.Column,
                        error.Column,
                        error.IsWarning ? Severity.Warning : Severity.Error));
            }

            Success = !Errors.Any();
        }
    }
}
