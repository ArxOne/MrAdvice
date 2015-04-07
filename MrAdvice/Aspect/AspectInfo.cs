#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Aspect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Annotation;

    /// <summary>
    /// Aspect, with pointcut and advices applied to it
    /// </summary>
    internal class AspectInfo
    {
        /// <summary>
        /// Gets the advices applied to pointcut in this aspect.
        /// </summary>
        /// <value>
        /// The advices.
        /// </value>
        public IList<AdviceInfo> Advices { get; private set; }
        /// <summary>
        /// Gets the advised method.
        /// </summary>
        /// <value>
        /// The advised method.
        /// </value>
        public MethodBase AdvisedMethod { get; private set; }
        /// <summary>
        /// Gets the pointcut method.
        /// </summary>
        /// <value>
        /// The pointcut method.
        /// </value>
        public MethodInfo PointcutMethod { get; private set; }
        /// <summary>
        /// Gets the pointcut property, if any (if method is related to property).
        /// </summary>
        /// <value>
        /// The pointcut property.
        /// </value>
        public PropertyInfo PointcutProperty { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is pointcut property setter.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pointcut property setter; otherwise, <c>false</c>.
        /// </value>
        public bool IsPointcutPropertySetter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectInfo" /> class.
        /// </summary>
        /// <param name="advices">The advices.</param>
        /// <param name="pointcutMethod">The pointcut method.</param>
        /// <param name="advisedMethod">The advised method.</param>
        /// <param name="pointcutProperty">The pointcut property.</param>
        /// <param name="isPointcutPropertySetter">if set to <c>true</c> [is pointcut property setter].</param>
        public AspectInfo(IEnumerable<AdviceInfo> advices, MethodInfo pointcutMethod, MethodBase advisedMethod, PropertyInfo pointcutProperty, bool isPointcutPropertySetter)
            : this(advices, pointcutMethod, advisedMethod)
        {
            PointcutProperty = pointcutProperty;
            IsPointcutPropertySetter = isPointcutPropertySetter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectInfo" /> class.
        /// </summary>
        /// <param name="advices">The advices.</param>
        /// <param name="pointcutMethod">The pointcut method.</param>
        /// <param name="advisedMethod">The advised method.</param>
        public AspectInfo(IEnumerable<AdviceInfo> advices, MethodInfo pointcutMethod, MethodBase advisedMethod)
        {
            Advices = advices.OrderByDescending(a => Priority.GetLevel(a.Advice)).ToArray();
            PointcutMethod = pointcutMethod;
            AdvisedMethod = advisedMethod;
        }

        /// <summary>
        /// Adds the advice and returns a new AspectInfo.
        /// </summary>
        /// <param name="adviceInfo">The advice information.</param>
        /// <returns></returns>
        public AspectInfo AddAdvice(AdviceInfo adviceInfo)
        {
            return new AspectInfo(Advices.Concat(new[] { adviceInfo }), PointcutMethod, AdvisedMethod, PointcutProperty, IsPointcutPropertySetter);
        }

        /// <summary>
        /// Applies the generic parameters.
        /// </summary>
        /// <param name="genericArguments">The generic parameters.</param>
        /// <returns></returns>
        public AspectInfo ApplyGenericParameters(Type[] genericArguments)
        {
            if (genericArguments == null)
                return this;

            // cast here is safe, because we have generic parameters, meaning we're not in a ctor
            return new AspectInfo(Advices,
                MakeGenericMethod(PointcutMethod, genericArguments),
                MakeGenericMethod((MethodInfo)AdvisedMethod, genericArguments))
            {
                PointcutProperty = PointcutProperty,
                IsPointcutPropertySetter = IsPointcutPropertySetter
            };
        }

        /// <summary>
        /// Makes a method from generic definition (type and method).
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="genericArguments">The generic arguments.</param>
        /// <returns></returns>
        private static MethodInfo MakeGenericMethod(MethodInfo methodInfo, Type[] genericArguments)
        {
            // two steps in this method.
            // 1. make generic type
            // 2. make generic method
            // genericArguments are given for type and method (each one taking what it needs)
            int typeGenericParametersCount = 0;

            // first, the type
            var declaringType = methodInfo.DeclaringType;
            if (declaringType.IsGenericTypeDefinition)
            {
                var typeGenericArguments = genericArguments.Take(typeGenericParametersCount = declaringType.GetGenericArguments().Length).ToArray();
                declaringType = declaringType.MakeGenericType(typeGenericArguments);
                // method needs to be discovered again.
                // Fortunately, it can be found by its handle.
                methodInfo = (MethodInfo)MethodBase.GetMethodFromHandle(methodInfo.MethodHandle, declaringType.TypeHandle);
            }
            // then, the method
            if (methodInfo.IsGenericMethodDefinition)
            {
                var methodGenericArguments = genericArguments.Skip(typeGenericParametersCount).ToArray();
                methodInfo = methodInfo.MakeGenericMethod(methodGenericArguments);
            }
            return methodInfo;
        }
    }
}