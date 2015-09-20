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
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Contains a marshal by ref type wrapped in an <see cref="AppDomain"/> loaded with a bunch of extra assemblies
    /// </summary>
    /// <typeparam name="TEntry">The type of the entry.</typeparam>
    public class WorkerAppDomain<TEntry> : IDisposable
        where TEntry : MarshalByRefObject
    {
        private readonly string _directory;
        private readonly AppDomain _appDomain;

        /// <summary>
        /// Gets the entry.
        /// </summary>
        /// <value>
        /// The entry.
        /// </value>
        public TEntry Entry { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerAppDomain{TEntry}"/> class.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="entryAssemblyFileName">Name of the entry assembly file.</param>
        /// <param name="extraAssemblyFileNames">The extra assembly file names.</param>
        public WorkerAppDomain(string directory, string entryAssemblyFileName, params string[] extraAssemblyFileNames)
        {
            _appDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            var entryAssemblyName = Load(directory, entryAssemblyFileName).FullName;
            foreach (var assemblyFileName in new[] { entryAssemblyFileName }.Concat(extraAssemblyFileNames))
                Load(directory, assemblyFileName);
            Entry = (TEntry)_appDomain.CreateInstanceAndUnwrap(entryAssemblyName, typeof(TEntry).FullName);
            _directory = directory;
        }

        /// <summary>
        /// Loads the specified assembly (by directory and file name).
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="assemblyFileName">Name of the assembly file.</param>
        /// <returns></returns>
        private Assembly Load(string directory, string assemblyFileName)
        {
            var assemblyBytes = File.ReadAllBytes(Path.Combine(directory, assemblyFileName));
            return _appDomain.Load(assemblyBytes);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            AppDomain.Unload(_appDomain);
            try
            {
                Directory.Delete(_directory, true);
            }
            catch (IOException) { }
        }
    }
}
