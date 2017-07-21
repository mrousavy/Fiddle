namespace Fiddle.Compilers.Implementation {
    public class ExecutionProperties : IExecutionProperties {
        public ExecutionProperties(long timeout) {
            Timeout = timeout;
        }

        private const int DefaultTimeout = 10000;

        public ExecutionProperties() : this(DefaultTimeout) { }
        public long Timeout { get; }
    }
}