#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.IO
{
    using System;

    /// <summary>
    /// A simple (but convenient [at least to me]) logger
    /// </summary>
    internal static class Logger
    {
        public static Action<string> LogInfo { get; set; }
        public static Action<string> LogWarning { get; set; }

        /// <summary>
        /// Writes to debug (does not write in release).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteDebug(string format, params object[] args)
        {
#if DEBUG
            LogInfo("..." + string.Format(format, args));
#endif
        }

        /// <summary>
        /// Writes to message window.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void Write(string format, params object[] args)
        {
            LogInfo(string.Format(format, args));
        }

        /// <summary>
        /// Writes a warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteWarning(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }
    }
}
