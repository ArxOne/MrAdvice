#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Advice
{
    using System;

    /// <summary>
    /// Base context to type
    /// </summary>
    public abstract class WeavingContext
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeavingContext"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        protected WeavingContext(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// Adds the public automatic property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public abstract bool AddPublicAutoProperty(string name);
    }
}
