#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Advice;
    using Aspect;
    using Pointcut;
    using Threading;
    using Utility;

    /// <summary>
    /// Exposes a method to start advisors chain call
    /// This class is public, since call from generated assembly.
    /// Semantically, it is internal.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static partial class Invocation
    {
        internal static readonly IDictionary<MethodBase, AspectInfo> AspectInfos = new Dictionary<MethodBase, AspectInfo>();

        private static readonly RuntimeTypeHandle VoidTypeHandle = typeof(void).TypeHandle;

        private static readonly AdviceInfo[] NoAdvice = new AdviceInfo[0];

        /// <summary>
        /// Runs a method interception.
        /// We use a static method here, if one day we want to reuse Invocations or change mecanism,
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="innerMethodHandle">The inner method handle.</param>
        /// <param name="typeHandle">The type handle.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <param name="genericArguments">The generic arguments (to static type and/or method) in a single array.</param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static object ProceedAdvice(object target, object[] parameters, RuntimeMethodHandle methodHandle, RuntimeMethodHandle innerMethodHandle, RuntimeTypeHandle typeHandle,
            bool abstractedTarget, Type[] genericArguments)
        {
            var methodBase = GetMethodFromHandle(methodHandle, typeHandle);
            var innerMethod = innerMethodHandle != methodHandle ? GetMethodFromHandle(innerMethodHandle, typeHandle) : null;

            var aspectInfo = GetAspectInfo(methodBase, methodHandle, innerMethod, innerMethodHandle, abstractedTarget, genericArguments);

            // this is the case with auto implemented interface
            var advisedInterface = target as AdvisedInterface;
            if (advisedInterface != null)
                aspectInfo = aspectInfo.AddAdvice(new AdviceInfo(advisedInterface.Advice));

            foreach (var advice in aspectInfo.Advices.Select(a => a.Advice).Distinct())
                InjectIntroducedFields(advice, methodBase.DeclaringType);

            // from here, we build an advice chain, with at least one final advice: the one who calls the method
            var adviceValues = new AdviceValues(target, aspectInfo.AdvisedMethod.DeclaringType, parameters);
            // at least there is one context
            AdviceContext adviceContext = new InnerMethodContext(adviceValues, aspectInfo.PointcutMethod);
            foreach (var advice in aspectInfo.Advices.Reverse())
            {
                // aspects are processed from highest to lowest level, so they are linked here in the opposite order
                // 3. as parameter
                if (advice.ParameterAdvice != null && advice.ParameterIndex.HasValue)
                {
                    var parameterIndex = advice.ParameterIndex.Value;
                    var parameterInfo = GetParameterInfo(aspectInfo.AdvisedMethod, parameterIndex);
                    adviceContext = new ParameterAdviceContext(advice.ParameterAdvice, parameterInfo, parameterIndex, adviceValues, adviceContext);
                }
                // 2. as method
                if (advice.MethodAdvice != null)
                    adviceContext = new MethodAdviceContext(advice.MethodAdvice, aspectInfo.AdvisedMethod, adviceValues, adviceContext);
                // 2b. as async method
                if (advice.AsyncMethodAdvice != null)
                    adviceContext = new MethodAsyncAdviceContext(advice.AsyncMethodAdvice, aspectInfo.AdvisedMethod, adviceValues, adviceContext);
                // 1. as property
                if (advice.PropertyAdvice != null && aspectInfo.PointcutProperty != null)
                    adviceContext = new PropertyAdviceContext(advice.PropertyAdvice, aspectInfo.PointcutProperty, aspectInfo.IsPointcutPropertySetter, adviceValues, adviceContext);
            }

            // if the method is no task, then we return immediately
            // (and the adviceTask is completed)
            var adviceTask = adviceContext.Invoke();

            var advisedMethodInfo = aspectInfo.AdvisedMethod as MethodInfo;
            var returnType = advisedMethodInfo?.ReturnType;
            // no Task means aspect was sync, so everything already ended
            // TODO: this is actually not true, since an async method can be void :frown:
            if (adviceTask == null || returnType == null || !typeof(Task).GetAssignmentReader().IsAssignableFrom(returnType))
            {
                adviceTask?.Wait();
                return adviceValues.ReturnValue;
            }

            // otherwise, see if it is a Task or Task<>

            // Task is simple too: the advised method is a subtask,
            // so the advice is completed after the method is completed too
            if (returnType == typeof(Task))
                return adviceTask;

            // only Task<> left here
            var taskType = returnType.GetTaskType();
            // a reflection equivalent of ContinueWith<TNewResult>, but this TNewResult, under taskType is known only at run-time
            return adviceTask.ContinueWith(t => GetResult(t, adviceValues), taskType);
        }

        /// <summary>
        /// Gets the method from handle.
        /// </summary>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="typeHandle">The type handle.</param>
        /// <returns></returns>
        private static MethodBase GetMethodFromHandle(RuntimeMethodHandle methodHandle, RuntimeTypeHandle typeHandle)
        {
            if (typeHandle.Equals(VoidTypeHandle))
                return MethodBase.GetMethodFromHandle(methodHandle);
            return MethodBase.GetMethodFromHandle(methodHandle, typeHandle);
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <param name="advisedTask">The advised task.</param>
        /// <param name="adviceValues">The advice values.</param>
        /// <returns></returns>
        private static object GetResult(Task advisedTask, AdviceValues adviceValues)
        {
            // when faulted here, no need to go further
            if (advisedTask.IsFaulted)
                throw FlattenException(advisedTask.Exception).PreserveStackTrace();

            // otherwise check inner value
            var returnValue = (Task)adviceValues.ReturnValue;
            if (returnValue.IsFaulted)
                throw FlattenException(returnValue.Exception).PreserveStackTrace();
            return returnValue.GetResult();
        }

        /// <summary>
        /// Flattens the exception (removes aggregate exception).
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        private static Exception FlattenException(Exception e)
        {
            var a = e as AggregateException;
            if (a == null)
                return e;
            return a.InnerException;
        }

        /// <summary>
        /// Gets the aspect information.
        /// </summary>
        /// <param name="method">The method base.</param>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="innerMethod">The inner method.</param>
        /// <param name="innerMethodHandle">The inner method handle.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <param name="genericArguments">The generic arguments.</param>
        /// <returns></returns>
        private static AspectInfo GetAspectInfo(MethodBase method, RuntimeMethodHandle methodHandle, MethodBase innerMethod, RuntimeMethodHandle innerMethodHandle, bool abstractedTarget, Type[] genericArguments)
        {
            AspectInfo aspectInfo;
            lock (AspectInfos)
            {
                if (!AspectInfos.TryGetValue(method, out aspectInfo))
                {
                    // this is to handle one special case:
                    // when an assembly advice is applied at assembly level, its ctor is also advised
                    // and getting its attributes, it creates an infinite loop
                    // so since an advice won't advise itself anyway
                    // we create an empty AspectInfo
                    AspectInfos[method] = new AspectInfo(NoAdvice, (MethodInfo)innerMethod, innerMethodHandle, method, methodHandle);
                    // the innerMethod is always a MethodInfo, because we created it, so this cast here is totally safe
                    AspectInfos[method] = aspectInfo = CreateAspectInfo(method, methodHandle, (MethodInfo)innerMethod, innerMethodHandle, abstractedTarget);
                }
            }

            aspectInfo = aspectInfo.ApplyGenericParameters(genericArguments);
            return aspectInfo;
        }

        /// <summary>
        /// Gets the parameter information.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <param name="parameterIndex">Index of the parameter.</param>
        /// <returns></returns>
        private static ParameterInfo GetParameterInfo(MethodBase methodBase, int parameterIndex)
        {
            if (parameterIndex >= 0)
                return methodBase.GetParameters()[parameterIndex];
            return ((MethodInfo)methodBase).ReturnParameter;
        }

        /// <summary>
        /// Processes the info advices.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        // ReSharper disable once UnusedMember.Global
        public static void ProcessInfoAdvices(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                ProcessInfoAdvices(type);
        }

        /// <summary>
        /// Processes the info advices.
        /// </summary>
        /// <param name="type">The type.</param>
        public static void ProcessInfoAdvices(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            foreach (var methodInfo in type.GetMembersReader().GetMethods(bindingFlags))
                ProcessMethodInfoAdvices(methodInfo);
            foreach (var constructorInfo in type.GetMembersReader().GetConstructors(bindingFlags))
                ProcessMethodInfoAdvices(constructorInfo);
            foreach (var propertyInfo in type.GetMembersReader().GetProperties(bindingFlags))
            {
                ProcessMethodInfoAdvices(propertyInfo.GetGetMethod());
                ProcessMethodInfoAdvices(propertyInfo.GetSetMethod());
                ProcessPropertyInfoAdvices(propertyInfo);
            }
        }

        /// <summary>
        /// Processes the info advices for MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        private static void ProcessMethodInfoAdvices(MethodBase methodInfo)
        {
            if (methodInfo == null)
                return;
            var methodInfoAdvices = methodInfo.GetAttributes<IMethodInfoAdvice>();
            foreach (var methodInfoAdvice in methodInfoAdvices)
            {
                // actually, introducing fields does not make sense here, until we introduce static fields
                SafeInjectIntroducedFields(methodInfoAdvice as IAdvice, methodInfo.DeclaringType);
                methodInfoAdvice.Advise(new MethodInfoAdviceContext(methodInfo));
            }
        }

        /// <summary>
        /// Processes the info advices for PropertyInfo.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        private static void ProcessPropertyInfoAdvices(PropertyInfo propertyInfo)
        {
            var propertyInfoAdvices = propertyInfo.GetAttributes<IPropertyInfoAdvice>();
            foreach (var propertyInfoAdvice in propertyInfoAdvices)
            {
                SafeInjectIntroducedFields(propertyInfoAdvice as IAdvice, propertyInfo.DeclaringType);
                propertyInfoAdvice.Advise(new PropertyInfoAdviceContext(propertyInfo));
            }
        }

        /// <summary>
        /// Creates the method call context, given a calling method and the inner method name.
        /// </summary>
        /// <param name="method">The method information.</param>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="innerMethod">Name of the inner method.</param>
        /// <param name="innerMethodHandle">The inner method handle.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <returns></returns>
        private static AspectInfo CreateAspectInfo(MethodBase method, RuntimeMethodHandle methodHandle, MethodInfo innerMethod, RuntimeMethodHandle innerMethodHandle, bool abstractedTarget)
        {
            Tuple<PropertyInfo, bool> relatedPropertyInfo;
            if (innerMethod == null && !abstractedTarget)
                method = FindInterfaceMethod(method);
            var advices = GetAdvices<IAdvice>(method, out relatedPropertyInfo);
            if (relatedPropertyInfo == null)
                return new AspectInfo(advices, innerMethod, innerMethodHandle, method, methodHandle);
            return new AspectInfo(advices, innerMethod, innerMethodHandle, method, methodHandle, relatedPropertyInfo.Item1, relatedPropertyInfo.Item2);
        }

        /// <summary>
        /// Finds the interface method implemented.
        /// </summary>
        /// <param name="implementationMethodBase">The method base.</param>
        /// <returns></returns>
        private static MethodBase FindInterfaceMethod(MethodBase implementationMethodBase)
        {
            // GetInterfaceMap is unfortunately unavailable in PCL :'(
            // ReSharper disable once PossibleNullReferenceException
            var i = implementationMethodBase.DeclaringType.GetAssignmentReader().GetInterfaces().SingleOrDefault();
            var parameterInfos = implementationMethodBase.GetParameters();
            var m = i.GetMembersReader().GetMethod(implementationMethodBase.Name, parameterInfos.Select(p => p.ParameterType).ToArray());
            return m;
        }

        /// <summary>
        /// Gets all advices available for this method.
        /// </summary>
        /// <typeparam name="TAdvice">The type of the advice.</typeparam>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="relatedPropertyInfo">The related property information.</param>
        /// <returns></returns>
        internal static IEnumerable<AdviceInfo> GetAllAdvices<TAdvice>(MethodBase targetMethod, out Tuple<PropertyInfo, bool> relatedPropertyInfo)
            where TAdvice : class, IAdvice
        {
            // inheritance hierarchy
            var typeAndParents = targetMethod.DeclaringType.GetSelfAndEnclosing().SelectMany(t => t.GetSelfAndParents()).Distinct().ToArray();
            // assemblies
            var assemblyAndParents = typeAndParents.Select(t => t.GetInformationReader().Assembly).Distinct();

            // advices down to method
            IEnumerable<AdviceInfo> allAdvices = assemblyAndParents.SelectMany(a => a.GetAttributes<TAdvice>())
                .Union(typeAndParents.SelectMany(t => t.GetAttributes<TAdvice>()))
                .Union(targetMethod.GetAttributes<TAdvice>()).Select(CreateAdvice)
                .ToArray();

            // optional from property
            relatedPropertyInfo = GetPropertyInfo(targetMethod);
            if (relatedPropertyInfo != null)
                allAdvices = allAdvices.Union(relatedPropertyInfo.Item1.GetAttributes<TAdvice>().Select(CreateAdvice)).ToArray();
            // now separate parameters
            var parameterAdvices = allAdvices.Where(a => a.Advice is IParameterAdvice).ToArray();
            allAdvices = allAdvices.Where(a => !(a.Advice is IParameterAdvice)).ToArray();

            // and parameters (not union but concat, because same attribute may be applied at different levels)
            // ... indexed parameters
            var parameters = targetMethod.GetParameters();
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var index = parameterIndex;
                allAdvices = allAdvices.Concat(parameterAdvices.Select(p => CreateAdviceIndex(p.Advice, parameterIndex)))
                    .Concat(parameters[parameterIndex].GetAttributes<TAdvice>().Select(a => CreateAdviceIndex(a, index)));
                // evaluate now
                allAdvices = allAdvices.ToArray();
            }
            // ... return value
            var methodInfo = targetMethod as MethodInfo;
            if (methodInfo != null)
            {
                allAdvices = allAdvices.Concat(parameterAdvices.Select(p => CreateAdviceIndex(p.Advice, -1)))
                    .Concat(methodInfo.ReturnParameter.GetAttributes<TAdvice>().Select(a => CreateAdviceIndex(a, -1)));
            }

            return allAdvices;
        }

        internal static IEnumerable<AdviceInfo> GetAdvices<TAdvice>(MethodBase targetMethod, out Tuple<PropertyInfo, bool> relatedPropertyInfo)
            where TAdvice : class, IAdvice
        {
            // first of all, get all advices that should apply here
            var allAdvices = GetAllAdvices<TAdvice>(targetMethod, out relatedPropertyInfo);
            // then, keep only selected advices (by their declaring pointcuts)
            var selectedAdvices = allAdvices.Where(a => Select(targetMethod, a));
            // if method declares an exclusion, use it
            var adviceSelector = GetAdviceSelector(targetMethod);
            // remaining advices
            var advices = selectedAdvices.Where(a => SelectAdvice(a, adviceSelector));
            return advices;
        }

        private static bool SelectAdvice(AdviceInfo adviceInfo, PointcutSelector exclusionRules)
        {
            return exclusionRules.Select(adviceInfo.Advice.GetType().FullName, null);
        }

        private static AdviceInfo CreateAdvice<TAdvice>(TAdvice advice)
            where TAdvice : class, IAdvice
        {
            return new AdviceInfo(advice);
        }

        private static AdviceInfo CreateAdviceIndex<TAdvice>(TAdvice advice, int index)
            where TAdvice : class, IAdvice
        {
            return new AdviceInfo(advice, index);
        }

        /// <summary>
        /// Gets the PropertyInfo, related to a method.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns>A tuple with the PropertyInfo and true is method is a setter (false for a getter)</returns>
        private static Tuple<PropertyInfo, bool> GetPropertyInfo(MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null || !methodInfo.IsSpecialName)
                return null;

            var isGetter = methodInfo.Name.StartsWith("get_");
            var isSetter = methodInfo.Name.StartsWith("set_");
            if (!isGetter && !isSetter)
                return null;

            // now try to find the property
            // ReSharper disable once PossibleNullReferenceException
            var propertyInfo = methodInfo.DeclaringType.GetMembersReader().GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(p => p.GetGetMethod(true) == methodInfo || p.GetSetMethod(true) == methodInfo);
            if (propertyInfo == null)
                return null; // this should never happen

            return Tuple.Create(propertyInfo, isSetter);
        }
    }
}
