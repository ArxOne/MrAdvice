#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver
{
    using System;

    /// <summary>
    /// A simple (but convenient [at least to me]) logger
    /// </summary>
    internal class Logger
    {
        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }

        /// <summary>
        /// Writes to debug (does not write in release).
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WriteDebug(string format, params object[] args)
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
        public void Write(string format, params object[] args)
        {
            LogInfo(string.Format(format, args));
        }

        /// <summary>
        /// Writes a warning.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void WriteWarning(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }
    }
}
