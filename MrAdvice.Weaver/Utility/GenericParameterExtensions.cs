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
        public static GenericParam Clone(this GenericParam genericParameter, MethodDef methodDefinition)
        {
            var newGenericParameter = new GenericParamUser(genericParameter.Number, genericParameter.Flags, genericParameter.Name);
            return newGenericParameter;
        }


    }
}
