using System;
using System.IO;
using System.Threading;
using System.Windows;
using Fiddle.Compilers;

namespace Fiddle.UI {
    public class FiddleGlobals : IGlobals {
        /// <summary>
        ///     A delegate for invoking any parameterless action
        /// </summary>
        /// <param name="action">The action to invoke</param>
        public delegate void Invoke(Action action);

        public FiddleGlobals(Editor caller) {
            Editor = caller;
            App = Application.Current;
            Random = new Random();
            RunUi = App.Dispatcher.Invoke;
            CurrentThread = Thread.CurrentThread;
        }

        /// <summary>
        ///     A random number object
        /// </summary>
        public Random Random { get; }

        /// <summary>
        ///     The current Editor window
        /// </summary>
        public Editor Editor { get; set; }

        /// <summary>
        ///     This WPF Application
        /// </summary>
        public Application App { get; set; }

        /// <summary>
        ///     Run the given action on the main thread of this WPF App
        /// </summary>
        public Invoke RunUi { get; set; }

        /// <summary>
        ///     A Stream to redirect console output to a stringbuilder
        /// </summary>
        public StringWriter Console { get; set; }

        /// <summary>
        ///     The Thread this object was created on
        /// </summary>
        public Thread CurrentThread { get; }
    }
}