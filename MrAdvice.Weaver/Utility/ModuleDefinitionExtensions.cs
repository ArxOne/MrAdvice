#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using IO;
    using Mono.Cecil;
    using Mono.Cecil.Rocks;

    /// <summary>
    /// Extensions to IAssemblyResolver
    /// </summary>
    internal static class ModuleDefinitionExtensions
    {
        /// <summary>
        /// Enumerates from self to references, recursively, by layers, with a maximum depth (good luck otherwise).
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="ignoreSystem">if set to <c>true</c> [ignore system].</param>
        /// <param name="maximumDepth">The maximum depth (or -1 to search all).</param>
        /// <returns></returns>
        public static IEnumerable<ModuleDefinition> GetSelfAndReferences(this ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver,
            bool ignoreSystem, int maximumDepth)
        {
            var remainingModules = new List<Tuple<ModuleDefinition, int>> { Tuple.Create(moduleDefinition, maximumDepth) };
            var discoveredNames = new HashSet<string> { moduleDefinition.FullyQualifiedName };
            while (remainingModules.Count > 0)
            {
                var thisModule = remainingModules[0].Item1;
                var depth = remainingModules[0].Item2;
                remainingModules.RemoveAt(0);

                yield return thisModule;

                // now, recurse
                if (depth != 0)
                {
                    foreach (var referencedModule in thisModule.GetReferencedModules(assemblyResolver, ignoreSystem))
                    {
                        var fullyQualifiedName = referencedModule.FullyQualifiedName;
                        if (!discoveredNames.Contains(fullyQualifiedName))
                        {
                            remainingModules.Add(Tuple.Create(referencedModule, depth - 1));
                            discoveredNames.Add(fullyQualifiedName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the references for the given module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="ignoreSystem">if set to <c>true</c> [ignore system].</param>
        /// <returns></returns>
        private static IEnumerable<ModuleDefinition> GetReferencedModules(this ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver, bool ignoreSystem)
        {
            var assemblyDefinitions = moduleDefinition.AssemblyReferences
                .Where(ar => !ignoreSystem || !IsSystem(ar))
                .Select(r => TryResolve(assemblyResolver, r)).Where(a => a != null);
            return assemblyDefinitions.Select(a => a.MainModule);
        }

        /// <summary>
        /// Determines whether the specified name reference references a system assembly.
        /// </summary>
        /// <param name="nameReference">The name reference.</param>
        /// <returns></returns>
        private static bool IsSystem(AssemblyNameReference nameReference)
        {
            if (nameReference.Name == "mscorlib")
                return true;
            var prefix = nameReference.Name.Split('.')[0];
            return prefix == "System" || prefix == "Microsoft";
        }

        private static readonly IDictionary<AssemblyNameReference, AssemblyDefinition> ResolutionCache =
            new Dictionary<AssemblyNameReference, AssemblyDefinition>();

        /// <summary>
        /// Tries to resolve the given assembly.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="assemblyNameReference">The assembly name reference.</param>
        /// <returns></returns>
        private static AssemblyDefinition TryResolve(IAssemblyResolver assemblyResolver, AssemblyNameReference assemblyNameReference)
        {
            lock (ResolutionCache)
            {
                AssemblyDefinition assemblyDefinition;
                if (ResolutionCache.TryGetValue(assemblyNameReference, out assemblyDefinition))
                    return assemblyDefinition;

                ResolutionCache[assemblyNameReference] = assemblyDefinition = TryLoad(assemblyResolver, assemblyNameReference);
                return assemblyDefinition;
            }
        }

        /// <summary>
        /// Tries the load the referenced assembly.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="assemblyNameReference">The assembly name reference.</param>
        /// <returns></returns>
        private static AssemblyDefinition TryLoad(IAssemblyResolver assemblyResolver, AssemblyNameReference assemblyNameReference)
        {
            try
            {
                Logger.WriteDebug("TryLoad '{0}'", assemblyNameReference.FullName);
                return assemblyResolver.Resolve(assemblyNameReference);
            }
            catch (FileNotFoundException)
            { }
            return null;
        }

        public static MethodReference SafeImport(this ModuleDefinition moduleDefinition, MethodReference methodReference)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(methodReference);
        }

        public static MethodReference SafeImport(this ModuleDefinition moduleDefinition, MethodBase methodBase)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(methodBase);
        }

        public static FieldReference SafeImport(this ModuleDefinition moduleDefinition, FieldInfo fieldInfo)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(fieldInfo);
        }

        public static TypeReference SafeImport(this ModuleDefinition moduleDefinition, TypeReference typeReference)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(typeReference);
        }

        public static TypeReference SafeImport(this ModuleDefinition moduleDefinition, Type type)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(type);
        }

        public static CustomAttribute CreateCustomAttribute(this ModuleDefinition moduleDefinition, TypeReference customAttributeType)
        {
            var constructor = customAttributeType.Resolve().GetConstructors().Single();
            return new CustomAttribute(moduleDefinition.SafeImport(constructor), new byte[] { 1, 0, 0, 0 });
        }
    }
}
