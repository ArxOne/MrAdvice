#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Advice;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using IO;
    using StitcherBoy.Logging;
    using System.Reflection;
    using Utility;

    internal class WeaverMethodWeavingContext : MethodWeavingContext
    {
        private readonly TypeDef _typeDefinition;
        private readonly WeavingContext _context;
        private readonly TypeResolver _typeResolver;
        private readonly ILogging _logging;

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public virtual IEnumerable<string> Properties => _typeDefinition.Properties.Select(p => p.Name.String);

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaverMethodWeavingContext" /> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition (type being built).</param>
        /// <param name="type">The type (original type).</param>
        /// <param name="targetMethodName">Name of the target method.</param>
        /// <param name="context">The context.</param>
        /// <param name="typeResolver">The type resolver.</param>
        /// <param name="logging">The logging.</param>
        public WeaverMethodWeavingContext(TypeDef typeDefinition, Type type, string targetMethodName, WeavingContext context, TypeResolver typeResolver, ILogging logging)
            : base(type, targetMethodName)
        {
            _typeDefinition = typeDefinition;
            _context = context;
            _typeResolver = typeResolver;
            _logging = logging;
        }

        /// <summary>
        /// Adds the public automatic property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        public override void AddPublicAutoProperty(string propertyName, Type propertyType)
        {
            var moduleDefinition = _typeDefinition.Module;
            _typeDefinition.AddPublicAutoProperty(propertyName, moduleDefinition.Import(propertyType), moduleDefinition, _typeResolver);
        }

        /// <summary>
        /// Adds the initializer.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        public override void AddInitializer(Action<object> initializer)
        {
            AddInitializer(initializer, false);
        }

        /// <summary>
        /// Adds an initializer once to all ctors (even if the method is called several times).
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        public override void AddInitializerOnce(Action<object> initializer)
        {
            AddInitializer(initializer, true);
        }

        private void AddInitializer(Action<object> initializer, bool once)
        {
            Add(initializer.Method, once, _typeDefinition.FindConstructors().Where(c => !c.IsStaticConstructor));
        }

        public override void AddMethod(string methodName, Action<object> method, Type interfaceImplicitImplementation = null)
        {
            throw new NotImplementedException();
        }

        public override void AddMethod<T1>(string methodName, Action<object, T1> method, Type interfaceImplicitImplementation = null)
        {
            throw new NotImplementedException();
        }

        public override void AddMethod<T1, T2>(string methodName, Action<object, T1, T2> method, Type interfaceImplicitImplementation = null)
        {
            throw new NotImplementedException();
        }

        public override void AddFinalizer(Action<object> finalizer)
        {
            var typeFinalizer = _typeDefinition.GetOrCreateFinalizer(_typeResolver);
            Add(finalizer.Method, typeFinalizer);
        }

        private void Add(MethodInfo methodInfo, MethodDef holderMethod)
        {
            Add(methodInfo, false, new[] { holderMethod });
        }
        private void Add(MethodInfo methodInfo, bool once, IEnumerable<MethodDef> holderMethods)
        {
            if (!IsMethodValid(methodInfo))
                return;

            var methodReference = _typeDefinition.Module.SafeImport(methodInfo);
            foreach (var holderMethod in holderMethods)
            {
                if (once && holderMethod.Body.Instructions.Any(i => i.OpCode == OpCodes.Call && methodReference.SafeEquivalent(i.Operand as IMethod, true)))
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

        private bool IsMethodValid(MethodInfo methodInfo)
        {
            bool valid = true;
            if (!methodInfo.IsStatic)
            {
                _logging.WriteError("The method {0}.{1} must be static", methodInfo.DeclaringType.FullName, methodInfo.Name);
                valid = false;
            }
            return valid;
        }
    }
}
