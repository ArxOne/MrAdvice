#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Lazy loading assembly holder
    /// </summary>
    public class AssemblyHolder
    {
        private readonly Func<Assembly> _loader;
        private Assembly _assembly;

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public Assembly Assembly
        {
            get
            {
                if (_assembly == null)
                    _assembly = _loader();
                return _assembly;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyHolder"/> class.
        /// </summary>
        /// <param name="loader">The loader.</param>
        public AssemblyHolder(Func<Assembly> loader)
        {
            _loader = loader;
        }
    }
}
