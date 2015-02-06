#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Utility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions to type
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Enumerates from type to topmost parent
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSelfAndParents(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}
