#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.IO
{
    using StitcherBoy.Logging;
    using Utility;

    internal class MultiLogging : ILogging
    {
        private readonly ILogging[] _loggings;

        public MultiLogging(params ILogging[] loggings)
        {
            _loggings = loggings;
        }

        public void Write(string format, params object[] parameters) => _loggings.ForAll(l => l.Write(format, parameters));

        public void WriteWarning(string format, params object[] parameters) => _loggings.ForAll(l => l.WriteWarning(format, parameters));

        public void WriteError(string format, params object[] parameters) => _loggings.ForAll(l => l.WriteError(format, parameters));

        public void WriteDebug(string format, params object[] parameters) => _loggings.ForAll(l => l.WriteDebug(format, parameters));
    }
}
