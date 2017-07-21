namespace Fiddle.Compilers.Implementation {
    public class ExecutionProperties : IExecutionProperties {
        private const int DefaultTimeout = 10000;

        public ExecutionProperties(long timeout) {
            Timeout = timeout;
        }

        public ExecutionProperties() : this(DefaultTimeout) { }
        public long Timeout { get; }
    }
}