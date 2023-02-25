using System;
using System.Collections.Generic;
using System.Reflection;

namespace ArxOne.MrAdvice.Advice.Builder;

public interface ITypeWeaver
{
    Type Type { get; }

    /// <summary>
    /// Gets the properties.
    /// </summary>
    /// <value>
    /// The properties.
    /// </value>
    IEnumerable<string> Properties { get; }

    void AddAutoProperty(string propertyName, Type propertyType, MethodAttributes attributes);

    /// <summary>Adds an initializer to all ctors (at the end of them).</summary>
    /// <param name="initializer">The initializer, which receives the instance as parameter.</param>
    /// <param name="flags"></param>
    void AddInitializer(Delegate initializer, WeaverAddFlags flags = WeaverAddFlags.Default);

    /// <summary>
    /// Adds a finalizer action.
    /// </summary>
    /// <param name="finalizer">The finalizer.</param>
    /// <param name="flags">The flags.</param>
    void AddFinalizer(Delegate finalizer, WeaverAddFlags flags = WeaverAddFlags.Default);

    void AddMethod(string methodName, Delegate method, WeaverAddFlags flags = WeaverAddFlags.Default,
        MethodAttributes methodAttributes = MethodAttributes.Public);
}
