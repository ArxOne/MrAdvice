#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

using System;
using System.Reflection;

namespace ArxOne.MrAdvice.Advice.Builder;

public static class TypeWeaverExtensions
{
    /// <summary>
    /// Adds an initializer to all ctors (at the end of them).
    /// </summary>
    /// <param name="typeWeaver">The type weaver.</param>
    /// <param name="initializer">The initializer, which receives the instance as parameter.</param>
    /// <param name="flags">The flags.</param>
    public static void AfterConstructors(this ITypeWeaver typeWeaver, Action<object> initializer, WeaverAddFlags flags = WeaverAddFlags.Default)
    {
        typeWeaver.AfterConstructors((Delegate)initializer, flags);
    }

    /// <summary>
    /// Adds a finalizer action.
    /// </summary>
    /// <param name="typeWeaver">The type weaver.</param>
    /// <param name="finalizer">The finalizer.</param>
    /// <param name="flags">The flags.</param>
    public static void AfterFinalizer(this ITypeWeaver typeWeaver, Action<object> finalizer, WeaverAddFlags flags = WeaverAddFlags.Default)
    {
        typeWeaver.AfterFinalizer((Delegate)finalizer, flags);
    }

    public static void AfterMethod(this ITypeWeaver typeWeaver, string methodName, Action<object> method,
        WeaverAddFlags flags = WeaverAddFlags.Default, MethodAttributes methodAttributes = MethodAttributes.Public)
    {
        typeWeaver.AfterMethod(methodName, (Delegate)method, flags);
    }
}