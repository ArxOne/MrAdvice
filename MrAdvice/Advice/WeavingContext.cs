#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Reflection;
using ArxOne.MrAdvice.Advice.Builder;

namespace ArxOne.MrAdvice.Advice;

using System;

/// <summary>
/// Base context to type
/// </summary>
public class WeavingContext
{
    /// <summary>
    /// Gets the type.
    /// </summary>
    /// <value>
    /// The type.
    /// </value>
    public Type Type => TypeWeaver.Type;

    public ITypeWeaver TypeWeaver { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeavingContext"/> class.
    /// </summary>
    /// <param name="typeWeaver"></param>
    public WeavingContext(ITypeWeaver typeWeaver)
    {
        TypeWeaver = typeWeaver;
    }

    /// <summary>
    /// Adds the public automatic property.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyType"></param>
    /// <returns></returns>
    [Obsolete("Use TypeWeaver")]
    public void AddPublicAutoProperty(string propertyName, Type propertyType) => TypeWeaver.AddAutoProperty(propertyName, propertyType, MethodAttributes.Public);

    /// <summary>
    /// Adds an initializer to all ctors (at the end of them).
    /// </summary>
    /// <param name="initializer">The initializer, which receives the instance as parameter.</param>
    [Obsolete("Use TypeWeaver")]
    public void AddInitializer(Action<object> initializer) => TypeWeaver.AfterConstructors(initializer, WeaverAddFlags.Default);

    /// <summary>
    /// Adds an initializer once to all ctors (even if the method is called several times).
    /// </summary>
    /// <param name="initializer">The initializer.</param>
    [Obsolete("Use TypeWeaver")]
    public void AddInitializerOnce(Action<object> initializer) => TypeWeaver.AfterConstructors(initializer, WeaverAddFlags.Once);
}
