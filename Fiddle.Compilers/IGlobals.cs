using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fiddle.Compilers
{
    /// <summary>
    /// Global variables to be used in a language (e.g. C#) (e.g. for redirecting Console to a StreamWriter)
    /// </summary>
    public interface IGlobals {
        StringWriter Console { get; }
    }
}
