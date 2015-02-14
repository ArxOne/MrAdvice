#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    partial class AspectWeaver
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private static string GetPropertyName(string methodName)
        {
            return methodName.Substring(4);
        }

        /// <summary>
        /// Gets the name of the property inner getter.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static string GetPropertyInnerGetterName(string propertyName)
        {
            return string.Format("\u200B{0}.get", propertyName);
        }

        /// <summary>
        /// Gets the name of the property inner setter.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static string GetPropertyInnerSetterName(string propertyName)
        {
            return string.Format("\u200B{0}.set", propertyName);
        }

        /// <summary>
        /// Gets the name of the inner method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private static string GetInnerMethodName(string methodName)
        {
            return string.Format("{0}\u200B", methodName);
        }
    }
}
