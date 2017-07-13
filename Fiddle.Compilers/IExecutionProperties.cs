namespace Fiddle.Compilers
{
    /// <summary>
    /// Properties for execution
    /// </summary>
    public interface IExecutionProperties
    {
        /// <summary>
        /// The maximum time the execution can take 
        /// in milliseconds before being cancelled, 
        /// or -1 if no timeout is set.
        /// </summary>
        long Timeout { get; }
    }
}
