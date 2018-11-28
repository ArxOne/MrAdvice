#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Annotation;

    /// <summary>
    /// Extensions to IAdvice (one extension, actually)
    /// </summary>
    public static class AdviceExtensions
    {
        private static readonly IDictionary<Type, Type> Types = new Dictionary<Type, Type>();

        /// <summary>
        /// Creates a proxy around the given interface, and injects the given advice at all levels.
        /// When weaving an interface from another assembly, it is required to specify a reference <see cref="Assembly"/> or <see cref="Type"/>, otherwise MrAdvice won't find it
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="advice">The advice.</param>
        /// <param name="referenceAssembly">A reference <see cref="Assembly"/> where the implementation is weaved.</param>
        /// <param name="referenceType">A reference <see cref="Type"/> where the implementation is .</param>
        /// <returns></returns>
        public static TInterface Handle<TInterface>(this IAdvice advice, Assembly referenceAssembly = null, Type referenceType = null)
        {
            return (TInterface)Handle(advice, typeof(TInterface), referenceAssembly, referenceType);
        }

        /// <summary>
        /// Creates a proxy around the given interface, and injects the given advice at all levels.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="referenceAssembly">The reference assembly.</param>
        /// <param name="referenceType">Type of the reference.</param>
        /// <returns>An object implementing the requested interface</returns>
        public static object Handle(this IAdvice advice, Type interfaceType, Assembly referenceAssembly = null, Type referenceType = null)
        {
            var implementationType = GetImplementationType(interfaceType, referenceAssembly, referenceType);
            var implementation = (AdvisedInterface)Activator.CreateInstance(implementationType);
            implementation.Advice = advice;
            return implementation;
        }

        /// <summary>
        /// Gets the type of the implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="referenceAssembly">The reference assembly.</param>
        /// <param name="referenceType">Type of the reference.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Interface implementation was not found. Ensure that the Handle method is called directly (without reflection).</exception>
        private static Type GetImplementationType(Type interfaceType, Assembly referenceAssembly, Type referenceType)
        {
            lock (Types)
            {
                if (Types.TryGetValue(interfaceType, out var implementationType))
                    return implementationType;

                var interfaceAssembly = interfaceType.GetInformationReader().Assembly;
                var assemblies = new[] { interfaceAssembly, PlatformUtility.GetCallingAssembly(), referenceAssembly, referenceType?.GetInformationReader().Assembly };
                implementationType = (from assembly in assemblies
                                      where !(assembly is null)
                                      from t in assembly.GetTypes()
                                      where t.GetInformationReader().BaseType == typeof(AdvisedInterface)
                                      let i = t.GetAssignmentReader().GetInterfaces()
                                      where i.Contains(interfaceType)
                                      select t).FirstOrDefault();
                if (implementationType == null)
                    throw new ArgumentException("Interface implementation was not found. Ensure that the Handle<> method is called directly (without reflection) or that the interface is marked with [" + nameof(DynamicHandleAttribute) + "].");
                Types[interfaceType] = implementationType;
                return implementationType;
            }
        }
    }
}
