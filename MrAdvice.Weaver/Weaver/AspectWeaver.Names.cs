#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System.Linq;
    using dnlib.DotNet;

    partial class AspectWeaver
    {
        // \u200B was the best choice ever. However as a space, it was trimmed from names,
        // (VS generate fake assemblies for example), causing a problem.
        // This one is a "not so bad, but not as good"...
        private const string Marker = "\u2032";

        /// <summary>
        /// Gets the special owner name.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private static string GetSpecialOwnerName(string methodName)
        {
            return methodName.Split(new[] {'_'}, 2)[1];
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
        /// Gets the name of the event adder.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns></returns>
        private static string GetEventInnerAdderName(string eventName)
        {
            return $"{eventName}.add{Marker}";
        }

        /// <summary>
        /// Gets the name of the event removed.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns></returns>
        private static string GetEventInnerRemoverName(string eventName)
        {
            return $"{eventName}.remove{Marker}";
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

        private static string GetDelegateProceederName(string methodName, TypeDef declaringType)
        {
            for (int index = 1; ; index++)
            {
                var name = methodName + "\u2033";
                if (index > 1)
                    name += index;
                if (declaringType.Methods.All(m => m.Name != name))
                    return name;
            }
        }
    }
}