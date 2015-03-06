#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System.Collections.Generic;
    using Mono.Cecil;

    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Gets the self and parents.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <returns></returns>
        public static IEnumerable<TypeDefinition> GetSelfAndParents(this TypeDefinition typeDefinition)
        {
            for (; ; )
            {
                yield return typeDefinition;
                var baseType = typeDefinition.BaseType;
                if (baseType == null)
                    break;
                typeDefinition = baseType.Resolve();
            }
        }
    }
}