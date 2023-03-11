using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ArxOne.MrAdvice.Advice.Builder;
using ArxOne.MrAdvice.Utility;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using StitcherBoy.Logging;
using MethodAttributes = System.Reflection.MethodAttributes;

namespace ArxOne.MrAdvice.Weaver;

internal class TypeWeaver : ITypeWeaver
{
    private class Methods : IEnumerable<MethodDef>
    {
        private readonly ICollection<MethodDef> _methods;

        public Methods(MethodDef method)
        {
            _methods = new[] { method };
        }

        public Methods(IEnumerable<MethodDef> methods)
        {
            _methods = methods.ToArray();
        }

        public IEnumerator<MethodDef> GetEnumerator() => _methods.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator Methods(MethodDef method) => new(method);
    }

    private readonly TypeDef _typeDefinition;
    private readonly WeavingContext _context;
    private readonly TypeResolver _typeResolver;
    private readonly ILogging _logging;

    public Type Type { get; }

    /// <summary>
    /// Gets the properties.
    /// </summary>
    /// <value>
    /// The properties.
    /// </value>
    public virtual IEnumerable<string> Properties => _typeDefinition.Properties.Select(p => p.Name.String);

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeWeaver" /> class.
    /// </summary>
    /// <param name="typeDefinition">The type definition (type being built).</param>
    /// <param name="type">The type (original type).</param>
    /// <param name="context">The context.</param>
    /// <param name="typeResolver">The type resolver.</param>
    /// <param name="logging">The logging.</param>
    public TypeWeaver(TypeDef typeDefinition, Type type, WeavingContext context, TypeResolver typeResolver, ILogging logging)
    {
        Type = type;
        _typeDefinition = typeDefinition;
        _context = context;
        _typeResolver = typeResolver;
        _logging = logging;
    }

    public void AddAutoProperty(string propertyName, Type propertyType, MethodAttributes attributes)
    {
        var moduleDefinition = _typeDefinition.Module;
        _typeDefinition.AddAutoProperty(propertyName, moduleDefinition.Import(propertyType), moduleDefinition, _typeResolver, attributes);
    }

    public void AfterConstructors(Delegate initializer, WeaverAddFlags flags)
    {
        var ctors = _typeDefinition.FindConstructors().Where(c => !c.IsStaticConstructor);
        Add(initializer.Method, flags, new Methods(ctors));
    }

    public void AfterFinalizer(Delegate advice, WeaverAddFlags flags)
    {
        var typeFinalizer = _typeDefinition.GetOrCreateFinalizer(_typeResolver);
        Add(advice.Method, flags, typeFinalizer);
    }

    public void AfterMethod(string methodName, Delegate advice, WeaverAddFlags flags = WeaverAddFlags.Default)
    {
        var methods = _typeDefinition.FindMethods(methodName);
        Add(advice.Method, flags, new Methods(methods));
    }

    public void After(MethodInfo methodInfo, Delegate advice, WeaverAddFlags flags = WeaverAddFlags.Default)
    {
        var methodDef = _typeDefinition.FindMethodCheckBaseType(methodInfo);
        Add(advice.Method, flags, methodDef);
    }

    private void Add(MethodInfo methodInfo, WeaverAddFlags flags, Methods holderMethods)
    {
        if (!IsValid(methodInfo))
            return;

        var methodReference = _typeDefinition.Module.SafeImport(methodInfo);
        foreach (var holderMethod in holderMethods)
        {
            if (!CanAddMethod(methodInfo, flags, holderMethod))
                continue;

            var instructions = new Instructions(holderMethod.Body, _typeDefinition.Module);
            // last instruction is a RET, so move just before it
            if (holderMethod.Name == "Finalize")
            {
                var finallyExceptionHandler = holderMethod.Body.ExceptionHandlers.First(e => e.HandlerType == ExceptionHandlerType.Finally);
                instructions.Cursor = holderMethod.Body.Instructions.IndexOf(finallyExceptionHandler.TryEnd) - 1;
            }
            else
                instructions.Cursor = instructions.Count - 1;

            instructions.Emit(OpCodes.Ldarg_0);
            instructions.Emit(OpCodes.Call, methodReference);
        }
    }

    private bool CanAddMethod(MethodInfo methodInfo, WeaverAddFlags flags, MethodDef methodDef)
    {
        var methodReference = _typeDefinition.Module.SafeImport(methodInfo);
        if (!flags.HasFlag(WeaverAddFlags.Once))
            return false;
        if (methodDef.Body.Instructions.Any(i => i.OpCode == OpCodes.Call && methodReference.SafeEquivalent(i.Operand as IMethod, true)))
            return false;
        return true;
    }

    private bool IsValid(MethodInfo methodInfo)
    {
        return IsAllValid(methodInfo).Aggregate(true, (isValid, singleResult) => singleResult || isValid);
    }

    private IEnumerable<bool> IsAllValid(MethodInfo methodInfo)
    {
        yield return IsStatic(methodInfo);
        yield return IsPublic(methodInfo);
    }

    private bool IsStatic(MethodInfo methodInfo)
    {
        if (methodInfo.IsStatic)
            return true;
        _logging.WriteError("The method {0}.{1} must be static", methodInfo.DeclaringType.FullName, methodInfo.Name);
        return false;
    }

    private bool IsPublic(MethodInfo methodInfo)
    {
        if (methodInfo.IsPublic)
            return true;
        _logging.WriteError("The method {0}.{1} must be public", methodInfo.DeclaringType.FullName, methodInfo.Name);
        return false;
    }
}
