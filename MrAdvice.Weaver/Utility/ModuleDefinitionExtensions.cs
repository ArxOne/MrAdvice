#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Mono.Cecil;

    /// <summary>
    /// Extensions to IAssemblyResolver
    /// </summary>
    internal static class ModuleDefinitionExtensions
    {
        /// <summary>
        /// Enumerates from self to references, recursively, by layers, with a maximum depth (good luck otherwise).
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="maximumDepth">The maximum depth (or -1 to search all).</param>
        /// <returns></returns>
        public static IEnumerable<ModuleDefinition> GetSelfAndReferences(this  ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver, int maximumDepth)
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
                    foreach (var referencedModule in thisModule.GetReferencedModules(assemblyResolver))
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
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        private static IEnumerable<ModuleDefinition> GetReferencedModules(this ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
        {
            var assemblyDefinitions = moduleDefinition.AssemblyReferences.Select(r => TryResolve(assemblyResolver, r)).Where(a => a != null);
            return assemblyDefinitions.Select(a => a.MainModule);
        }

        /// <summary>
        /// Tries to resolve the given assembly.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        /// <param name="assemblyNameReference">The assembly name reference.</param>
        /// <returns></returns>
        private static AssemblyDefinition TryResolve(IAssemblyResolver assemblyResolver, AssemblyNameReference assemblyNameReference)
        {
            try
            {
                return assemblyResolver.Resolve(assemblyNameReference);
            }
            catch (FileNotFoundException)
            { }
            return null;
        }
    }
}
