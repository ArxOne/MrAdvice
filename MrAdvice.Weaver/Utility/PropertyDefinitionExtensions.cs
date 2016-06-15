#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using dnlib.DotNet;

    /// <summary>
    /// Extensions to property definition
    /// </summary>
    internal static class PropertyDefinitionExtensions
    {
        /// <summary>
        /// Determines whether the specified property definition is public.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        /// <returns></returns>
        public static bool HasAnyPublic(this PropertyDef  propertyDefinition)
        {
            if (propertyDefinition.GetMethod != null && propertyDefinition.GetMethod.IsPublic)
                return true;
            if (propertyDefinition.SetMethod != null && propertyDefinition.SetMethod.IsPublic)
                return true;
            return false;
        }
    }
}
