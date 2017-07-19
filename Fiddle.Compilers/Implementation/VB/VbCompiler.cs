using Microsoft.VisualBasic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#pragma warning disable 618

namespace Fiddle.Compilers.Implementation.VB
{
    public class VbCompiler : ICompiler
    {
        public IExecutionProperties ExecuteProperties { get; }
        public ICompilerProperties CompilerProperties { get; }
        public string SourceCode { get; set; }
        public ICompileResult CompileResult { get; private set; }
        public IExecuteResult ExecuteResult { get; private set; }
        public string[] Imports { get; set; }
        public Language Language { get; } = Language.Vb;

        private VBCodeProvider Provider { get; set; }
        private ICodeCompiler CodeCompiler { get; set; }
        private CompilerParameters Parameters { get; set; }
        private Assembly ScriptAssembly { get; set; }

        public VbCompiler(string code) : this(code, new ExecutionProperties(), new CompilerProperties()) { }

        public VbCompiler(string code, IExecutionProperties execProps, ICompilerProperties compProps, string[] imports = null)
        {
            SourceCode = code;
            ExecuteProperties = execProps;
            CompilerProperties = compProps;
            if (imports == null)
                LoadReferences();
            else
                Imports = imports;

            Create();
        }

        /// <summary>
        /// Load all references/namespaces that can be used in the script environment
        /// </summary>
        public void LoadReferences()
        {
            //Get all namespaces from this assembly (own project, own library, ..)
            IEnumerable<string> ownNamespaces = Assembly.GetExecutingAssembly().GetTypes()
                .Select(t => t.Namespace)
                .Distinct();

            List<string> allNamespaces = new List<string>();

            //Get all .NET defined types
            IEnumerable<Type[]> types = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => Assembly.GetCallingAssembly().GetReferencedAssemblies().Select(ra => ra.FullName).Contains(a.FullName))
                .Select(a => a.GetExportedTypes());

            //Add all types where namespace is not own namespace (no own references)
            foreach (Type[] type in types)
            {
                allNamespaces.AddRange(type
                    .Select(t => t.Namespace)
                    .Where(n => n != null && !ownNamespaces.Contains(n))
                    .Distinct());
            }

            Imports = allNamespaces.ToArray();
        }

        private void Create()
        {
            Provider = new VBCodeProvider();
            CodeCompiler = Provider.CreateCompiler();
            Parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                MainClass = "VbFiddle",
                OutputAssembly = "VbFiddleAssembly"
            };
            Parameters.ReferencedAssemblies.Add("System.dll");
            Parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            Parameters.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            Parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            Parameters.ReferencedAssemblies.Add("System.Data.dll");
            Parameters.ReferencedAssemblies.Add("System.Deployment.dll");
            Parameters.ReferencedAssemblies.Add("System.Xml.dll");
            //Parameters.ReferencedAssemblies.AddRange(Imports);
        }

        public async Task<ICompileResult> Compile()
        {
            TaskCompletionSource<ICompileResult> tcs = new TaskCompletionSource<ICompileResult>();

            //bad hack for creating an "async" method:
            new Thread(() =>
            {
                //Init
                CompilerResults results = null;

                //Actual compilation
                Stopwatch sw = Stopwatch.StartNew();
                Thread compileThread = new Thread(() =>
                {
                    results = CodeCompiler.CompileAssemblyFromSource(Parameters, SourceCode);
                });
                compileThread.Start();
                bool graceful = compileThread.Join((int)CompilerProperties.Timeout); //Wait until compile Thread finishes with given timeout
                sw.Stop();

                if (!graceful)
                    throw new CompileException("The compilation timed out!");

                if (results.Errors.Count < 1)
                    ScriptAssembly = results.CompiledAssembly;

                CompilerErrorCollection errors = results.Errors;
                if (errors == null)
                    throw new CompileException("The compiler Thread was not returning any diagnostics!");

                //Build compile result object
                tcs.SetResult(new VbCompileResult(
                    sw.ElapsedMilliseconds,
                    SourceCode,
                    errors));
            }).Start();

            ICompileResult compileResult = await tcs.Task;
            CompileResult = compileResult;
            return compileResult;
        }

        public async Task<IExecuteResult> Execute()
        {
            await Compile();
            if (!CompileResult.Success)
            {
                return new VbExecuteResult(-1, null, null, CompileResult, new CompileException("The compilation was not successful!"));
            }

            object returnValue = null;
            Exception exception = null;

            Stopwatch sw = Stopwatch.StartNew();

            Thread thread = new Thread(() =>
            {
                try
                {
                    returnValue = ScriptAssembly.EntryPoint == null
                        ? ScriptAssembly.DefinedTypes.Last().DeclaredMethods.First().Invoke(null, null)
                        : ScriptAssembly.EntryPoint.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.Start();
            bool graceful = thread.Join((int)ExecuteProperties.Timeout);

            sw.Stop();

            if (graceful)
            {
                IExecuteResult result = new VbExecuteResult(
                    sw.ElapsedMilliseconds,
                    null,
                    returnValue,
                    CompileResult,
                    exception);
                ExecuteResult = result;
                return result;
            }
            else
            {
                IExecuteResult result = new VbExecuteResult(
                    sw.ElapsedMilliseconds,
                    null,
                    null,
                    CompileResult,
                    new Exception("The execution timed out!"));
                ExecuteResult = result;
                return result;
            }
        }


        public bool CatchException(Exception ex)
        {
            return true;
        }
    }
}
