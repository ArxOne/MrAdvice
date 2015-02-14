#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.IO;
    using System.Linq;
    using IO;
    using Mono.Cecil;

    /// <summary>
    /// Type resolver allows to find a TypeDefinition given a full name
    /// </summary>
    internal class TypeResolver
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public Logger Logger { get; set; }

        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver { get; set; }

        /// <summary>
        /// Resolves the full name to a type definiton.
        /// Eventually searches through direct references
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public TypeDefinition Resolve(ModuleDefinition moduleDefinition, string fullName)
        {
            // no need to dig deeper than direct references
            return ResolveModule(moduleDefinition, fullName)
                   ?? ResolveReferences(moduleDefinition, fullName);
        }

        /// <summary>
        /// Tries to resolve the name in the given module.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        private static TypeDefinition ResolveModule(ModuleDefinition moduleDefinition, string fullName)
        {
            return moduleDefinition.GetTypes().FirstOrDefault(t => t.FullName == fullName);
        }

        /// <summary>
        /// Tries to resolve the name in the given module references.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        private TypeDefinition ResolveReferences(ModuleDefinition moduleDefinition, string fullName)
        {
            var assemblyDefinitions = moduleDefinition.AssemblyReferences.Select(TryResolve).Where(a => a != null).ToArray();
            return assemblyDefinitions.Select(a => ResolveAssembly(a, fullName)).FirstOrDefault(t => t != null);
        }

        private AssemblyDefinition TryResolve(AssemblyNameReference assemblyNameReference)
        {
            try
            {
                return AssemblyResolver.Resolve(assemblyNameReference);
            }
            catch (FileNotFoundException)
            { }
            return null;
        }

        /// <summary>
        /// Tries to resolve the name in all assembly types.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        private static TypeDefinition ResolveAssembly(AssemblyDefinition assemblyDefinition, string fullName)
        {
            return assemblyDefinition.Modules.Select(m => ResolveModule(m, fullName)).FirstOrDefault(t => t != null);
        }
    }
}
