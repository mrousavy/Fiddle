namespace Fiddle.Compilers.Implementation
{
    public class ExecutionProperties : IExecutionProperties
    {
        public long Timeout { get; }

        public ExecutionProperties(long timeout)
        {
            Timeout = timeout;
        }

        public ExecutionProperties() : this(-1) { }
    }
}
