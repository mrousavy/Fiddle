using System.IO;
using System.Threading;

namespace Fiddle.Compilers {
    /// <summary>
    ///     Global variables to be used in a language (e.g. C#) (e.g. for redirecting Console to a StreamWriter)
    /// </summary>
    public interface IGlobals {
        StringWriter Console { get; set; }
        Thread CurrentThread { get; }
    }
}