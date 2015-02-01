#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.Weavisor.Weaver.Utility
{
    using Mono.Cecil;

    /// <summary>
    /// Extensions to GenericParameter
    /// </summary>
    public static class GenericParameterExtensions
    {
        /// <summary>
        /// Clones the specified generic parameter.
        /// </summary>
        /// <param name="genericParameter">The generic parameter.</param>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        public static GenericParameter Clone(this GenericParameter genericParameter, MethodDefinition methodDefinition)
        {
            var newGenericParameter = new GenericParameter(methodDefinition);
            newGenericParameter.Attributes = genericParameter.Attributes;
            newGenericParameter.Name = genericParameter.Name;
            return newGenericParameter;
        }
    }
}
