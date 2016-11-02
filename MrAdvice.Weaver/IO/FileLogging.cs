#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.IO
{
    using System.Diagnostics;
    using System.IO;
    using StitcherBoy.Logging;

    public class FileLogging : ILogging
    {
        private readonly StreamWriter _streamWriter;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public FileLogging(string logFilePath)
        {
            _streamWriter = File.CreateText(logFilePath);
            _stopwatch.Start();
        }

        public void Write(string format, params object[] parameters)
        {
            var prefix = $@"[{_stopwatch.Elapsed:mm\:ss\.fff}] ";
            _streamWriter.WriteLine(prefix + format, parameters);
            _streamWriter.Flush();
        }

        public void WriteWarning(string format, params object[] parameters) => Write("! " + format, parameters);

        public void WriteError(string format, params object[] parameters) => Write("* " + format, parameters);

        public void WriteDebug(string format, params object[] parameters) => Write(". " + format, parameters);
    }
}
