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
        public IList<AdviceInfo> Advices { get; }
        /// <summary>
        /// Gets the advised method.
        /// </summary>
        /// <value>
        /// The advised method.
        /// </value>
        public MethodBase AdvisedMethod { get; }
        /// <summary>
        /// Gets the advised method handle.
        /// </summary>
        /// <value>
        /// The advised method handle.
        /// </value>
        public RuntimeMethodHandle AdvisedMethodHandle { get; }
        /// <summary>
        /// Gets the pointcut method.
        /// </summary>
        /// <value>
        /// The pointcut method.
        /// </value>
        public MethodInfo PointcutMethod { get; }
        /// <summary>
        /// Gets the pointcut method handle.
        /// </summary>
        /// <value>
        /// The pointcut method handle.
        /// </value>
        public RuntimeMethodHandle PointcutMethodHandle { get; }
        /// <summary>
        /// Gets the pointcut property, if any (if method is related to property).
        /// </summary>
        /// <value>
        /// The pointcut property.
        /// </value>
        public PropertyInfo PointcutProperty { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is pointcut property setter.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pointcut property setter; otherwise, <c>false</c>.
        /// </value>
        public bool IsPointcutPropertySetter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectInfo" /> class.
        /// </summary>
        /// <param name="advices">The advices.</param>
        /// <param name="pointcutMethod">The pointcut method.</param>
        /// <param name="pointcutMethodHandle">The pointcut method handle.</param>
        /// <param name="advisedMethod">The advised method.</param>
        /// <param name="advisedMethodHandle">The advised method handle.</param>
        /// <param name="pointcutProperty">The pointcut property.</param>
        /// <param name="isPointcutPropertySetter">if set to <c>true</c> [is pointcut property setter].</param>
        public AspectInfo(IEnumerable<AdviceInfo> advices, MethodInfo pointcutMethod, RuntimeMethodHandle pointcutMethodHandle, MethodBase advisedMethod, RuntimeMethodHandle advisedMethodHandle, PropertyInfo pointcutProperty, bool isPointcutPropertySetter)
            : this(advices, pointcutMethod, pointcutMethodHandle, advisedMethod, advisedMethodHandle)
        {
            PointcutProperty = pointcutProperty;
            IsPointcutPropertySetter = isPointcutPropertySetter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectInfo" /> class.
        /// </summary>
        /// <param name="advices">The advices.</param>
        /// <param name="pointcutMethod">The pointcut method.</param>
        /// <param name="pointcutMethodHandle">The pointcut method handle.</param>
        /// <param name="advisedMethod">The advised method.</param>
        /// <param name="advisedMethodHandle">The advised method handle.</param>
        public AspectInfo(IEnumerable<AdviceInfo> advices, MethodInfo pointcutMethod, RuntimeMethodHandle pointcutMethodHandle, MethodBase advisedMethod, RuntimeMethodHandle advisedMethodHandle)
        {
            Advices = advices.OrderByDescending(a => PriorityAttribute.GetLevel(a.Advice)).ToArray();
            PointcutMethod = pointcutMethod;
            PointcutMethodHandle = pointcutMethodHandle;
            AdvisedMethod = advisedMethod;
            AdvisedMethodHandle = advisedMethodHandle;
        }

        /// <summary>
        /// Adds the advice and returns a new AspectInfo.
        /// </summary>
        /// <param name="adviceInfo">The advice information.</param>
        /// <returns></returns>
        public AspectInfo AddAdvice(AdviceInfo adviceInfo)
        {
            return new AspectInfo(Advices.Concat(new[] { adviceInfo }), PointcutMethod, PointcutMethodHandle, AdvisedMethod, AdvisedMethodHandle, PointcutProperty, IsPointcutPropertySetter);
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
                (MethodInfo)MakeGenericMethod(PointcutMethod, PointcutMethodHandle, genericArguments), PointcutMethodHandle,
                MakeGenericMethod(AdvisedMethod, AdvisedMethodHandle, genericArguments), AdvisedMethodHandle, PointcutProperty, IsPointcutPropertySetter);
        }

        /// <summary>
        /// Makes a method from generic definition (type and method).
        /// </summary>
        /// <param name="methodBase">The method information.</param>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="genericArguments">The generic arguments.</param>
        /// <returns></returns>
        private static MethodBase MakeGenericMethod(MethodBase methodBase, RuntimeMethodHandle methodHandle, Type[] genericArguments)
        {
            // two steps in this method.
            // 1. make generic type
            // 2. make generic method
            // genericArguments are given for type and method (each one taking what it needs)
            int typeGenericParametersCount = 0;

            // first, the type
            var declaringType = methodBase.DeclaringType;
            // ReSharper disable once PossibleNullReferenceException
            if (declaringType.GetInformationReader().IsGenericTypeDefinition)
            {
                var typeGenericArguments = genericArguments.Take(typeGenericParametersCount = declaringType.GetAssignmentReader().GetGenericArguments().Length).ToArray();
                declaringType = declaringType.MakeGenericType(typeGenericArguments);
                // method needs to be discovered again.
                // Fortunately, it can be found by its handle.
                methodBase = MethodBase.GetMethodFromHandle(methodHandle, declaringType.TypeHandle);
            }
            // then, the method
            if (!methodBase.IsGenericMethodDefinition)
                return methodBase;
            // on generic parameters
            var methodGenericArguments = genericArguments.Skip(typeGenericParametersCount).ToArray();
            var methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
                return methodInfo.MakeGenericMethod(methodGenericArguments);
            return methodBase;
        }
    }
}