using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.CodeDom.Compiler;
using System.Text;

namespace Fiddle.Compilers.NET
{
    public class Compiler
    {
        private StringBuilder CodeBuilder { get; set; }

        public Compiler(string code)
        {
            CodeBuilder = new StringBuilder(code);
        }

        public Compiler(StringBuilder code)
        {
            CodeBuilder = code;
        }


        public CompileResult Compile()
        {
            CompileResult result = new CompileResult();

            CSharpCodeProvider provider = (CSharpCodeProvider)CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters parameters = new CompilerParameters { GenerateExecutable = true };
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, CodeBuilder.ToString());

            return result;
        }
    }
}
