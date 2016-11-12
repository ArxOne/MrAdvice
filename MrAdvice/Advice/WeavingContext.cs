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
        public Type Type { get; }

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
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        public abstract void AddPublicAutoProperty(string propertyName, Type propertyType);

        /// <summary>
        /// Adds an initializer to all ctors (at the end of them).
        /// </summary>
        /// <param name="initializer">The initializer, which receives the instance as parameter.</param>
        public abstract void AddInitializer(Action<object> initializer);
      
        /// <summary>
        /// Adds an initializer once to all ctors (even if the method is called several times).
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        public abstract void AddInitializerOnce(Action<object> initializer);
    }
}
