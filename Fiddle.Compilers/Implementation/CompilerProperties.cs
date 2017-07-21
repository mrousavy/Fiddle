namespace Fiddle.Compilers.Implementation {
    public class CompilerProperties : ICompilerProperties {
        public CompilerProperties(long timeout, string langVersion) {
            Timeout = timeout;
            LanguageVersion = langVersion;
        }

        private const int DefaultTimeout = 10000;

        public CompilerProperties() : this(DefaultTimeout, null) { }
        public long Timeout { get; }
        public string LanguageVersion { get; }
    }
}