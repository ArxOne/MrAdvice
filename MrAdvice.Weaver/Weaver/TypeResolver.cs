#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.Collections.Generic;
    using System.Linq;
    using Mono.Cecil;
    using Utility;

    /// <summary>
    /// Type resolver allows to find a TypeDefinition given a full name
    /// </summary>
    internal class TypeResolver
    {
        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver { get; set; }

        private readonly IDictionary<string, TypeDefinition> _resolvedTypes = new Dictionary<string, TypeDefinition>();

        /// <summary>
        /// Resolves the full name to a type definiton.
        /// Eventually searches through direct references
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        public TypeDefinition Resolve(ModuleDefinition moduleDefinition, string fullName)
        {
            lock (_resolvedTypes)
            {
                TypeDefinition typeDefinition;
                if (_resolvedTypes.TryGetValue(fullName, out typeDefinition))
                    return typeDefinition;

                // 2 levels, because of level of dependency:
                // - level 0: the assembly where advices are injected
                // - level 1: the assembly containing the advice
                // - level 2: the advices dependencies
                _resolvedTypes[fullName] = typeDefinition = Resolve(moduleDefinition, fullName, 2);
                return typeDefinition;
            }
        }

        /// <summary>
        /// Resolves the specified type by name in module and referenced (with a maximum depth)
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        private TypeDefinition Resolve(ModuleDefinition moduleDefinition, string fullName, int depth)
        {
            foreach (var referencedModule in moduleDefinition.GetSelfAndReferences(AssemblyResolver, depth))
            {
                var foundType = referencedModule.GetTypes().FirstOrDefault(t => t.FullName == fullName);
                if (foundType != null)
                    return foundType;
            }
            return null;
        }
    }
}
