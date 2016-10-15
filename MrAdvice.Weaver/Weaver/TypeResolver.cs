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
    using dnlib.DotNet;
    using IO;
    using Utility;

    /// <summary>
    /// Type resolver allows to find a TypeDefinition given a full name
    /// </summary>
    internal class TypeResolver
    {
        private const int Depth = 2;

        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver
        {
            get { return _assemblyResolver; }
            set
            {
                _assemblyResolver = value;
                _resolver = new Resolver(value);
            }
        }

        private readonly IDictionary<string, TypeDef> _resolvedTypesByName = new Dictionary<string, TypeDef>();
        private IAssemblyResolver _assemblyResolver;
        private IResolver _resolver;

        /// <summary>
        /// Resolves the full name to a type definiton.
        /// Eventually searches through direct references
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public TypeDef Resolve(ModuleDef moduleDefinition, string fullName)
        {
            lock (_resolvedTypesByName)
            {
                TypeDef typeDefinition;
                if (_resolvedTypesByName.TryGetValue(fullName, out typeDefinition))
                    return typeDefinition;

                // 2 levels, because of level of dependency:
                // - level 0: the assembly where advices are injected
                // - level 1: the assembly containing the advice
                // - level 2: the advices dependencies
                _resolvedTypesByName[fullName] = typeDefinition = Resolve(moduleDefinition, fullName, true, Depth);
                return typeDefinition;
            }
        }

        /// <summary>
        /// Resolves the specified type by name in module and referenced (with a maximum depth)
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="ignoreSystem">if set to <c>true</c> [ignore system].</param>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        private TypeDef Resolve(ModuleDef moduleDefinition, string fullName, bool ignoreSystem, int depth)
        {
            //Logger.WriteDebug("Trying to resolve in the following modules:");
            //foreach (var module in moduleDefinition.GetSelfAndReferences(AssemblyResolver, ignoreSystem, 10))
            //    Logger.WriteDebug($" {module.FullName}");
            //foreach (var assemblyRef in moduleDefinition.GetAssemblyRefs())
            //{
            //    Logger.WriteDebug($"Ref '{assemblyRef.FullName}' from '{moduleDefinition.FullName}'");
            //    var assemblyDef =   AssemblyResolver.Resolve(assemblyRef, moduleDefinition);
            //    if (assemblyDef == null)
            //        Logger.WriteError(" Can't resolve!");
            //    else
            //    {
            //        var m = assemblyDef.GetMainModule();
            //        Logger.WriteDebug($" Loaded {m.FullName}");
            //    }
            //}
            lock (_resolvedTypesByName)
            {
                var selfAndReferences = moduleDefinition.GetSelfAndReferences(AssemblyResolver, ignoreSystem, depth);
                return selfAndReferences.SelectMany(referencedModule => referencedModule.GetTypes()).FirstOrDefault(t => Matches(t, fullName));
            }
        }

        private static bool Matches(TypeDef type, string fullName)
        {
            //Logger.WriteDebug("Checking type {0}", type.FullName);
            return type.FullName == fullName;
        }

        /// <summary>
        /// Resolves the specified type to Cecil.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public TypeDef Resolve(ModuleDef moduleDefinition, Type type)
        {
            return Resolve(moduleDefinition, type.FullName);
        }

        /// <summary>
        /// Resolves the specified type definition or reference.
        /// </summary>
        /// <param name="typeDefOrRef">The type definition or reference.</param>
        /// <returns></returns>
        public TypeDef Resolve(ITypeDefOrRef typeDefOrRef)
        {
            if (typeDefOrRef == null)
            {
                Logger.LogWarning("null typeDefOrRef provided for resolution");
                return null;
            }
            lock (_resolvedTypesByName)
            {
                TypeDef typeDef;
                if (!_resolvedTypesByName.TryGetValue(typeDefOrRef.AssemblyQualifiedName, out typeDef))
                    _resolvedTypesByName[typeDefOrRef.AssemblyQualifiedName] = typeDef = DoResolve(typeDefOrRef);
                return typeDef;
            }
        }

        private TypeDef DoResolve(ITypeDefOrRef typeDefOrRef)
        {
            return AsTypeDef(typeDefOrRef) ?? ResolverResolve(typeDefOrRef) ?? FullResolve(typeDefOrRef);
        }


        private TypeDef AsTypeDef(ITypeDefOrRef typeDefOrRef)
        {
            return typeDefOrRef as TypeDef;
        }

        private TypeDef ResolverResolve(ITypeDefOrRef typeDefOrRef)
        {
            var typeRef = (TypeRef)typeDefOrRef;
            var typeDef = _resolver.Resolve(typeRef);
            return typeDef;
        }

        private TypeDef FullResolve(ITypeDefOrRef typeDefOrRef)
        {
            // this method is actually never called...
            // TODO: remove
            foreach (var reference in typeDefOrRef.Module.GetSelfAndReferences(AssemblyResolver, false, int.MaxValue))
            {
                var typeDef = reference.Find(typeDefOrRef);
                if (typeDef != null)
                    return typeDef;
            }
            return null;
        }
    }
}
