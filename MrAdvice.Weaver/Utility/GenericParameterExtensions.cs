#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Utility
{
    using System.Linq;
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
            newGenericParameter.GenericParamConstraints.AddRange(genericParameter.GenericParamConstraints.Select(c => c.Clone()));
            return newGenericParameter;
        }

        /// <summary>
        /// Clones the specified generic parameter constraint.
        /// </summary>
        /// <param name="genericParamConstraint">The generic parameter constraint.</param>
        /// <returns></returns>
        public static GenericParamConstraint Clone(this GenericParamConstraint genericParamConstraint)
        {
            return new GenericParamConstraintUser(genericParamConstraint.Constraint);
        }
    }
}
