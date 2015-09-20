#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.IO;

    public class LoadedAssembly : IDisposable
    {
        private readonly string _directory;

        public LoadedAssembly(string directory, string assemblyFileName)
        {
            _directory = directory;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_directory, true);
            }
            catch { }
        }
    }
}
