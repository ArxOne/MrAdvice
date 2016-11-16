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
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extensions to IAdvice (one extension, actually)
    /// </summary>
    public static class AdviceExtensions
    {
        private static readonly IDictionary<Type, Type> Types = new Dictionary<Type, Type>();

        /// <summary>
        /// Creates a proxy around the given interface, and injects the given advice at all levels.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="advice">The advice.</param>
        /// <returns></returns>
        public static TInterface Handle<TInterface>(this IAdvice advice)
        {
            return (TInterface)Handle(advice, typeof(TInterface));
        }

        /// <summary>
        /// Creates a proxy around the given interface, and injects the given advice at all levels.
        /// </summary>
        /// <param name="advice">The advice.</param>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <returns></returns>
        private static object Handle(this IAdvice advice, Type interfaceType)
        {
            var implementationType = GetImplementationType(interfaceType);
            var implementation = (AdvisedInterface)Activator.CreateInstance(implementationType);
            implementation.Advice = advice;
            return implementation;
        }

        /// <summary>
        /// Gets the type of the implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <returns></returns>
        private static Type GetImplementationType(Type interfaceType)
        {
            lock (Types)
            {
                Type implementationType;
                if (Types.TryGetValue(interfaceType, out implementationType))
                    return implementationType;

                implementationType = (from t in interfaceType.GetInformationReader().Assembly.GetTypes()
                                      where t.GetInformationReader().BaseType == typeof(AdvisedInterface)
                                      let i = t.GetAssignmentReader().GetInterfaces()
                                      where i.Contains(interfaceType)
                                      select t).FirstOrDefault();
                if (implementationType == null)
                    throw new ArgumentException("Interface implementation was not found. Ensure that the Handle<> method is called directly (without reflection).");
                Types[interfaceType] = implementationType;
                return implementationType;
            }
        }
    }
}
