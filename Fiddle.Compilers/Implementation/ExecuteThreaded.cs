using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Fiddle.Compilers.Implementation {
    internal class ExecuteThreaded<T> {
        /// <summary>
        ///     Execute a given Function on a seperate Thread and wait until any result is returned.
        /// </summary>
        /// <param name="function">The Function to execute</param>
        /// <param name="timeout">Timeout (in ms) until the Function terminates. (-1 indicating no timeout)</param>
        /// <returns>A result collection of important values</returns>
        internal static async Task<ThreadRunResult> Execute(Func<T> function, int timeout) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Stopwatch sw = null;
            bool graceful = false;
            Exception exception = null;
            var returnValue = default(T);

            //Spawn new thread so UI Thread isn't blocked
            new Thread(() => {
                //Execute on runThread so we can Join/Abort it
                var runThread = new Thread(() => {
                    try {
                        //Sync run - we're in a new thread anyway
                        returnValue = function.Invoke();
                    } catch (ThreadAbortException) {
                        //Join(int) timed out
                    } catch (Exception ex) {
                        exception = ex;
                    }
                });
                sw = Stopwatch.StartNew();
                runThread.Start();
                graceful = runThread.Join(timeout);
                sw.Stop();
                if (!graceful)
                    runThread.Abort();
                tcs.SetResult(true);
            }).Start();

            //Wait until anonymous thread sets result
            await tcs.Task;

            return new ThreadRunResult(returnValue, (int) sw.ElapsedMilliseconds, graceful, exception);
        }

        /// <summary>
        ///     Execute a given Function on a seperate Thread and wait until any result is returned.
        /// </summary>
        /// <param name="function">The async Function to execute</param>
        /// <param name="timeout">Timeout (in ms) until the Function terminates. (-1 indicating no timeout)</param>
        /// <returns>A result collection of important values</returns>
        internal static async Task<ThreadRunResult> Execute(Func<Task<T>> function, int timeout) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Stopwatch sw = null;
            bool graceful = false;
            Exception exception = null;
            var returnValue = default(T);

            //Spawn new thread so UI Thread isn't blocked
            new Thread(() => {
                //Execute on runThread so we can Join/Abort it
                var runThread = new Thread(() => {
                    try {
                        //Sync run - we're in a new thread anyway
                        returnValue = function.Invoke().GetAwaiter().GetResult();
                    } catch (ThreadAbortException) {
                        //Join(int) timed out
                    } catch (Exception ex) {
                        exception = ex;
                    }
                });
                sw = Stopwatch.StartNew();
                runThread.Start();
                graceful = runThread.Join(timeout);
                sw.Stop();
                if (!graceful)
                    runThread.Abort();
                tcs.SetResult(true);
            }).Start();

            //Wait until anonymous thread sets result
            await tcs.Task;

            return new ThreadRunResult(returnValue, (int) sw.ElapsedMilliseconds, graceful, exception);
        }


        internal struct ThreadRunResult {
            internal T ReturnValue { get; }
            internal int ElapsedMilliseconds { get; }
            internal bool Successful { get; }
            internal Exception Exception { get; }

            internal ThreadRunResult(T retVal, int elapsed, bool success, Exception exception) {
                ReturnValue = retVal;
                ElapsedMilliseconds = elapsed;
                Successful = success;
                Exception = exception;
            }
        }
    }
}