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
    public static void AddInitializer(this ITypeWeaver typeWeaver, Action<object> initializer, WeaverAddFlags flags = WeaverAddFlags.Default)
    {
        typeWeaver.AddInitializer((Delegate)initializer, flags);
    }

    /// <summary>
    /// Adds a finalizer action.
    /// </summary>
    /// <param name="typeWeaver">The type weaver.</param>
    /// <param name="finalizer">The finalizer.</param>
    /// <param name="flags">The flags.</param>
    public static void AddFinalizer(this ITypeWeaver typeWeaver, Action<object> finalizer, WeaverAddFlags flags = WeaverAddFlags.Default)
    {
        typeWeaver.AddFinalizer((Delegate)finalizer, flags);
    }

    public static void AddMethod(this ITypeWeaver typeWeaver, string methodName, Action<object> method,
        WeaverAddFlags flags = WeaverAddFlags.Default, MethodAttributes methodAttributes = MethodAttributes.Public)
    {
        typeWeaver.AddMethod(methodName, (Delegate)method, flags, methodAttributes);
    }

    public static void AddMethod<T1>(this ITypeWeaver typeWeaver, string methodName, Action<object, T1> method,
        WeaverAddFlags flags = WeaverAddFlags.Default, MethodAttributes methodAttributes = MethodAttributes.Public)
    {
        typeWeaver.AddMethod(methodName, (Delegate)method, flags, methodAttributes);
    }

    public static void AddMethod<T1, T2>(this ITypeWeaver typeWeaver, string methodName, Action<object, T1, T2> method,
        WeaverAddFlags flags = WeaverAddFlags.Default, MethodAttributes methodAttributes = MethodAttributes.Public)
    {
        typeWeaver.AddMethod(methodName, (Delegate)method, flags, methodAttributes);
    }
}