﻿namespace Fiddle.Compilers.Implementation
{
    public class CompilerProperties : ICompilerProperties
    {
        public long Timeout { get; }
        public string LanguageVersion { get; }

        public CompilerProperties(long timeout, string langVersion)
        {
            Timeout = timeout;
            LanguageVersion = langVersion;
        }

        public CompilerProperties() : this(-1, null) { }
    }
}