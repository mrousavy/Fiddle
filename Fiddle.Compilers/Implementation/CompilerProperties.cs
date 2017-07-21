namespace Fiddle.Compilers.Implementation {
    public class CompilerProperties : ICompilerProperties {
        private const int DefaultTimeout = 10000;

        public CompilerProperties(long timeout, string langVersion) {
            Timeout = timeout;
            LanguageVersion = langVersion;
        }

        public CompilerProperties() : this(DefaultTimeout, null) { }
        public long Timeout { get; }
        public string LanguageVersion { get; }
    }
}