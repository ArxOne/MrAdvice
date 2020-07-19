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
    using dnlib.DotNet;
    using StitcherBoy.Logging;
    using Weaver;

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
        /// <param name="logging">The logging.</param>
        /// <param name="isMainModule">if set to <c>true</c> [is main module].</param>
        /// <returns></returns>
        public static IEnumerable<ModuleDef> GetSelfAndReferences(this ModuleDef moduleDefinition, IAssemblyResolver assemblyResolver,
            bool ignoreSystem, int maximumDepth, ILogging logging, bool isMainModule)
        {
            var remainingModules = new List<Tuple<ModuleDef, int>> { Tuple.Create(moduleDefinition, maximumDepth) };
            var discoveredNames = new HashSet<string> { moduleDefinition.FullName };
            while (remainingModules.Count > 0)
            {
                var thisModule = remainingModules[0].Item1;
                var depth = remainingModules[0].Item2;
                remainingModules.RemoveAt(0);

                yield return thisModule;

                // now, recurse
                if (depth > 0)
                {
                    foreach (var referencedModule in thisModule.GetReferencedModules(assemblyResolver, ignoreSystem, logging, isMainModule))
                    {
                        var fullyQualifiedName = referencedModule.FullName;
                        if (!discoveredNames.Contains(fullyQualifiedName))
                        {
                            remainingModules.Add(Tuple.Create(referencedModule, depth - 1));
                            discoveredNames.Add(fullyQualifiedName);
                        }
                    }
                    isMainModule = false;
                }
            }
        }

        /// <summary>
        /// Gets the references for the given module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="ignoreSystem">if set to <c>true</c> [ignore system].</param>
        /// <param name="logging">The logging.</param>
        /// <param name="isMainModule">if set to <c>true</c> [is main module].</param>
        /// <returns></returns>
        private static IEnumerable<ModuleDef> GetReferencedModules(this ModuleDef moduleDefinition, IAssemblyResolver assemblyResolver, bool ignoreSystem, ILogging logging, bool isMainModule)
        {
            var assemblyDefinitions = moduleDefinition.GetAssemblyRefs()
                .Where(ar => !ignoreSystem || !IsSystem(ar))
                .Select(r => TryResolve(assemblyResolver, r, moduleDefinition, logging, isMainModule)).Where(a => a != null);
            return assemblyDefinitions.Select(a => a.GetMainModule());
        }

        /// <summary>
        /// Determines whether the specified name reference references a system assembly.
        /// </summary>
        /// <param name="nameReference">The name reference.</param>
        /// <returns></returns>
        public static bool IsSystem(this IAssembly nameReference)
        {
            if (nameReference.Name == "mscorlib")
                return true;
            var prefix = nameReference.Name.String.Split('.')[0];
            return prefix == "System" ;//|| prefix == "Microsoft";
        }

        private static readonly IDictionary<AssemblyRef, AssemblyDef> ResolutionCache =
            new Dictionary<AssemblyRef, AssemblyDef>();

        /// <summary>
        /// Tries to resolve the given assembly.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="assemblyNameReference">The assembly name reference.</param>
        /// <param name="source">The source.</param>
        /// <param name="logging">The logging.</param>
        /// <param name="directReference">if set to <c>true</c> [direct reference].</param>
        /// <returns></returns>
        private static AssemblyDef TryResolve(IAssemblyResolver assemblyResolver, AssemblyRef assemblyNameReference, ModuleDef source, ILogging logging, bool directReference)
        {
            lock (ResolutionCache)
            {
                if (ResolutionCache.TryGetValue(assemblyNameReference, out var assemblyDefinition))
                    return assemblyDefinition;

                ResolutionCache[assemblyNameReference] = assemblyDefinition = TryLoad(assemblyResolver, assemblyNameReference, source, logging, directReference);
                return assemblyDefinition;
            }
        }

        /// <summary>
        /// Tries the load the referenced assembly.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="assemblyNameReference">The assembly name reference.</param>
        /// <param name="source">The source.</param>
        /// <param name="logging">The logging.</param>
        /// <param name="directReference">if set to <c>true</c> [direct reference].</param>
        /// <returns></returns>
        private static AssemblyDef TryLoad(IAssemblyResolver assemblyResolver, AssemblyRef assemblyNameReference, ModuleDef source, ILogging logging, bool directReference)
        {
            try
            {
                logging.WriteDebug("TryLoad '{0}' from '{1}'", assemblyNameReference.FullName, source.FullName);
                var assemblyDef = assemblyResolver.Resolve(assemblyNameReference, source);
                if (assemblyDef == null && directReference)
                    logging.WriteWarning("Can't load '{0}'", assemblyNameReference.FullName);
                return assemblyDef;
            }
            catch (FileNotFoundException)
            { }
            logging.WriteError("Failed loading '{0}'", assemblyNameReference.FullName);
            return null;
        }

        public static IMethod SafeImport(this ModuleDef moduleDefinition, IMethod methodReference)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(methodReference);
        }

        public static IMethod SafeImport(this ModuleDef moduleDefinition, MethodBase methodBase)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(methodBase);
        }

        public static MemberRef SafeImport(this ModuleDef moduleDefinition, FieldInfo fieldInfo)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(fieldInfo);
        }

        public static TypeSig SafeImport(this ModuleDef moduleDefinition, TypeSig typeSig)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(typeSig);
        }

        public static TypeRef SafeImport(this ModuleDef moduleDefinition, Type type)
        {
            lock (moduleDefinition)
                return moduleDefinition.Import(type).ToTypeSig().TryGetTypeRef();
        }

        public static CustomAttribute CreateCustomAttribute(this ModuleDef moduleDefinition, TypeRef customAttributeType, TypeResolver typeResolver)
        {
            var constructor = typeResolver.Resolve(customAttributeType).FindConstructors().Single();
            var importedCtor = moduleDefinition.SafeImport(constructor);
            return new CustomAttribute((ICustomAttributeType)importedCtor, new byte[] { 1, 0, 0, 0 });
        }
    }
}
