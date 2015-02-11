#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System.Reflection;

    /// <summary>
    /// Advices for <see cref="PropertyInfo"/>
    /// </summary>
    public interface IPropertyInfoAdvice : IInfoAdvice
    {
        /// <summary>
        /// Invoked once per property, when assembly is loaded
        /// </summary>
        /// <param name="context">The property info advice context</param>
        void Advise(PropertyInfoAdviceContext context);
    }
}
