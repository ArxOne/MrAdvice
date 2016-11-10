#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Weaver
{
    partial class AspectWeaver
    {
        //private const string Marker = "\u200B";
        private const string Marker = "\u02B9";

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
            return $"{propertyName}.get{Marker}";
        }

        /// <summary>
        /// Gets the name of the property inner setter.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static string GetPropertyInnerSetterName(string propertyName)
        {
            return $"{propertyName}.set{Marker}";
        }

        /// <summary>
        /// Gets the name of the implementation type.
        /// </summary>
        /// <param name="interfaceName">Name of the interface.</param>
        /// <returns></returns>
        private static string GetImplementationTypeName(string interfaceName)
        {
            return $"{interfaceName}{Marker}";
        }

        /// <summary>
        /// Gets the name of the inner method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private static string GetInnerMethodName(string methodName)
        {
            return $"{methodName}{Marker}";
        }
    }
}
