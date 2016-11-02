#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.IO
{
    using StitcherBoy.Logging;

    public class DefaultLogging : ILogging
    {
        private readonly ILogging _logging;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogging"/> class.
        /// </summary>
        /// <param name="logging">The logging.</param>
        public DefaultLogging(ILogging logging)
        {
            _logging = logging;
        }

        public void Write(string format, params object[] parameters) => _logging.Write(format, parameters);

        public void WriteWarning(string format, params object[] parameters) => _logging.WriteWarning(format, parameters);

        public void WriteError(string format, params object[] parameters) => _logging.WriteError(format, parameters);

        public void WriteDebug(string format, params object[] parameters)
        {
#if DEBUG
            _logging.Write("..." + format, parameters);
#else
            _logging.WriteDebug(format, parameters);
#endif
        }
    }
}
