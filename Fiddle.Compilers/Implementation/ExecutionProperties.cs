namespace Fiddle.Compilers.Implementation {
    public class ExecutionProperties : IExecutionProperties {
        public ExecutionProperties(long timeout) {
            Timeout = timeout;
        }

        public ExecutionProperties() : this(-1) { }
        public long Timeout { get; }
    }
}