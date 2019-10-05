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
    using Annotation;
    using Aspect;
    using global::MrAdvice.Advice;
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
        internal static readonly IDictionary<RuntimeMethodHandle, IDictionary<RuntimeTypeHandle, AspectInfo>> AspectInfos
            = new Dictionary<RuntimeMethodHandle, IDictionary<RuntimeTypeHandle, AspectInfo>>();

        private static readonly RuntimeTypeHandle VoidTypeHandle = typeof(void).TypeHandle;

        private static readonly AdviceInfo[] NoAdvice = new AdviceInfo[0];

        /// <summary>
        /// Runs a method interception.
        /// This version is kept for compatibility, the new method to be colled is <see cref="ProceedAdvice2"/>
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="innerMethodHandle">The inner method handle.</param>
        /// <param name="typeHandle">The type handle.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <param name="genericArguments">The generic arguments.</param>
        /// <returns></returns>
        public static object ProceedAdvice(object target, object[] parameters, RuntimeMethodHandle methodHandle,
            RuntimeMethodHandle innerMethodHandle, RuntimeTypeHandle typeHandle, bool abstractedTarget, Type[] genericArguments)
        {
            return ProceedAdvice2(target, parameters, methodHandle, innerMethodHandle, methodHandle, typeHandle, abstractedTarget, genericArguments);
        }

        /// <summary>
        /// Runs a method interception.
        /// We use a static method here, if one day we want to reuse Invocations or change mecanism,
        /// it will be easier from C# code
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="innerMethodHandle">The inner method handle.</param>
        /// <param name="delegatableMethodHandle">The delegatable method handle.</param>
        /// <param name="typeHandle">The type handle.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <param name="genericArguments">The generic arguments (to static type and/or method) in a single array.</param>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static object ProceedAdvice2(object target, object[] parameters, RuntimeMethodHandle methodHandle, RuntimeMethodHandle innerMethodHandle,
            RuntimeMethodHandle delegatableMethodHandle, RuntimeTypeHandle typeHandle, bool abstractedTarget, Type[] genericArguments)
        {
            var aspectInfo = GetAspectInfo(methodHandle, innerMethodHandle, delegatableMethodHandle, typeHandle, abstractedTarget, genericArguments);

            // this is the case with auto implemented interface
            if (target is AdvisedInterface advisedInterface)
                aspectInfo = aspectInfo.AddAdvice(new AdviceInfo(advisedInterface.Advice));

            foreach (var advice in aspectInfo.Advices)
                InjectIntroducedFields(advice, aspectInfo.AdvisedMethod.DeclaringType);

            // from here, we build an advice chain, with at least one final advice: the one who calls the method
            var adviceValues = new AdviceValues(target, aspectInfo.AdvisedMethod.DeclaringType, parameters);
            // at least there is one context
            var adviceContext = CreateAdviceContext(adviceValues, aspectInfo);

            // if the method is no task, then we return immediately
            // (and the adviceTask is completed)
            var adviceTask = adviceContext.Invoke();

            var advisedMethodInfo = aspectInfo.AdvisedMethod as MethodInfo;
            var returnType = advisedMethodInfo?.ReturnType;
            // no Task means aspect was sync, so everything already ended
            // or it may also been an async void, meaning that we don't care about it
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

            // when the advised task is the same, no need to continue with something else
            if (adviceValues.ReturnValue == adviceTask)
                return adviceTask;

            // a reflection equivalent of ContinueWith<TNewResult>, but this TNewResult, under taskType is known only at run-time
            return adviceTask.ContinueWith(t => GetResult(t, adviceValues), taskType);
        }

        private static AdviceContext CreateAdviceContext(AdviceValues adviceValues, AspectInfo aspectInfo)
        {
            AdviceContext adviceContext = new InnerMethodContext(adviceValues, aspectInfo.PointcutMethod, aspectInfo.PointcutMethodDelegate);
            for (var adviceIndex = aspectInfo.Advices.Count - 1; adviceIndex >= 0; adviceIndex--)
            {
                var advice = aspectInfo.Advices[adviceIndex];
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
                // 1b. as event
                if (advice.EventAdvice != null && aspectInfo.PointcutEvent != null)
                    adviceContext = new EventAdviceContext(advice.EventAdvice, aspectInfo.PointcutEvent, aspectInfo.IsPointcutEventAdder, adviceValues, adviceContext);
            }

            return adviceContext;
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
            if (!(e is AggregateException a))
                return e;
            return a.InnerException;
        }

        /// <summary>
        /// Gets the aspect information.
        /// </summary>
        /// <param name="methodHandle">The method handle.</param>
        /// <param name="innerMethodHandle">The inner method handle.</param>
        /// <param name="delegatableMethodHandle">The delegatable method handle.</param>
        /// <param name="typeHandle">The type handle.</param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <param name="genericArguments">The generic arguments.</param>
        /// <returns></returns>
        private static AspectInfo GetAspectInfo(RuntimeMethodHandle methodHandle, RuntimeMethodHandle innerMethodHandle,
            RuntimeMethodHandle delegatableMethodHandle, RuntimeTypeHandle typeHandle, bool abstractedTarget, Type[] genericArguments)
        {
            AspectInfo aspectInfo;
            lock (AspectInfos)
            {
                aspectInfo = FindAspectInfo(methodHandle, typeHandle);
                if (aspectInfo == null)
                {
                    var methodBase = GetMethodFromHandle(methodHandle, typeHandle);
                    var innerMethod = innerMethodHandle != methodHandle
                        ? GetMethodFromHandle(innerMethodHandle, typeHandle)
                        : null;
                    var delegateMethod = delegatableMethodHandle != innerMethodHandle
                        ? PlatformUtility.CreateDelegate<ProceedDelegate>((MethodInfo)GetMethodFromHandle(delegatableMethodHandle, typeHandle))
                        : null;
                    // this is to handle one special case:
                    // when an assembly advice is applied at assembly level, its ctor is also advised
                    // and getting its attributes, it creates an infinite loop
                    // so since an advice won't advise itself anyway
                    // we create an empty AspectInfo
                    SetAspectInfo(methodHandle, typeHandle,
                        new AspectInfo(NoAdvice, (MethodInfo)innerMethod, innerMethodHandle, delegateMethod, methodBase, methodHandle));
                    // the innerMethod is always a MethodInfo, because we created it, so this cast here is totally safe
                    aspectInfo = CreateAspectInfo(methodBase, methodHandle, (MethodInfo)innerMethod, innerMethodHandle, delegateMethod, abstractedTarget);
                    SetAspectInfo(methodHandle, typeHandle, aspectInfo);
                }
            }

            aspectInfo = aspectInfo.ApplyGenericParameters(genericArguments);
            return aspectInfo;
        }

        private static AspectInfo FindAspectInfo(RuntimeMethodHandle methodHandle, RuntimeTypeHandle typeHandle)
        {
            if (!AspectInfos.TryGetValue(methodHandle, out var methodsByType))
                return null;

            methodsByType.TryGetValue(typeHandle, out var aspectInfo);
            return aspectInfo;
        }

        private static void SetAspectInfo(RuntimeMethodHandle methodHandle, RuntimeTypeHandle typeHandle, AspectInfo aspectInfo)
        {
            if (!AspectInfos.TryGetValue(methodHandle, out var methodsByType))
                AspectInfos[methodHandle] = methodsByType = new Dictionary<RuntimeTypeHandle, AspectInfo>();
            methodsByType[typeHandle] = aspectInfo;
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
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                              BindingFlags.Static;
            var typeAndAssemblyAdvices = type.GetAssembly().GetAttributes<IMethodInfoAdvice>()
                .Union(type.GetAttributes<IMethodInfoAdvice>()).ToArray();
            foreach (var methodInfo in type.GetMembersReader().GetMethods(bindingFlags))
                ProcessMethodInfoAdvices(methodInfo, typeAndAssemblyAdvices);
            foreach (var constructorInfo in type.GetMembersReader().GetConstructors(bindingFlags))
                ProcessMethodInfoAdvices(constructorInfo, typeAndAssemblyAdvices);
            foreach (var propertyInfo in type.GetMembersReader().GetProperties(bindingFlags))
            {
                ProcessMethodInfoAdvices(propertyInfo.GetGetMethod(), typeAndAssemblyAdvices);
                ProcessMethodInfoAdvices(propertyInfo.GetSetMethod(), typeAndAssemblyAdvices);
                ProcessPropertyInfoAdvices(propertyInfo);
            }
        }

        /// <summary>
        /// Processes the info advices for MethodInfo.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="typeAndAssemblyMethodInfoAdvices">The type and assembly method information advices.</param>
        private static void ProcessMethodInfoAdvices(MethodBase methodInfo, IEnumerable<IMethodInfoAdvice> typeAndAssemblyMethodInfoAdvices)
        {
            if (methodInfo == null)
                return;
            var methodInfoAdvices = typeAndAssemblyMethodInfoAdvices.Union(methodInfo.GetAttributes<IMethodInfoAdvice>()).OrderByDescending(PriorityAttribute.GetLevel);
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
        /// <param name="innerMethodDelegate"></param>
        /// <param name="abstractedTarget">if set to <c>true</c> [abstracted target].</param>
        /// <returns></returns>
        private static AspectInfo CreateAspectInfo(MethodBase method, RuntimeMethodHandle methodHandle, MethodInfo innerMethod, RuntimeMethodHandle innerMethodHandle, ProceedDelegate innerMethodDelegate, bool abstractedTarget)
        {
            if (innerMethod == null && !abstractedTarget)
                method = FindInterfaceMethod(method);
            var advices = GetAdvices<IAdvice>(method, out var relatedPropertyInfo, out var relatedEventInfo);
            if (relatedPropertyInfo != null)
                return new AspectInfo(advices, innerMethod, innerMethodHandle, innerMethodDelegate, method, methodHandle, relatedPropertyInfo.Item1, relatedPropertyInfo.Item2);
            if (relatedEventInfo != null)
                return new AspectInfo(advices, innerMethod, innerMethodHandle, innerMethodDelegate, method, methodHandle, relatedEventInfo.Item1, relatedEventInfo.Item2);
            return new AspectInfo(advices, innerMethod, innerMethodHandle, innerMethodDelegate, method, methodHandle);
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
            var interfaces = implementationMethodBase.DeclaringType.GetAssignmentReader().GetInterfaces();
            var parameterInfos = implementationMethodBase.GetParameters();
            var parametersTypes = parameterInfos.Select(p => p.ParameterType).ToArray();
            foreach (var @interface in interfaces)
            {
                var method = @interface.GetMembersReader().GetMethod(implementationMethodBase.Name, parametersTypes);
                if (method != null)
                    return method;
            }

            return null;
        }

        /// <summary>
        /// Gets all advices available for this method.
        /// </summary>
        /// <typeparam name="TAdvice">The type of the advice.</typeparam>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="relatedPropertyInfo">The related property information.</param>
        /// <param name="relatedEventInfo">The related event information.</param>
        /// <returns></returns>
        internal static IEnumerable<AdviceInfo> GetAllAdvices<TAdvice>(MethodBase targetMethod,
            out Tuple<PropertyInfo, bool> relatedPropertyInfo, out Tuple<EventInfo, bool> relatedEventInfo)
            where TAdvice : class, IAdvice
        {
            // inheritance hierarchy
            var typeAndParents = targetMethod.DeclaringType.GetSelfAndEnclosing()
                .SelectMany(t => t.GetSelfAndAncestors())
                .Distinct()
                .ToArray();
            // assemblies
            var assemblyAndParents = typeAndParents.Select(t => t.GetInformationReader().Assembly).Distinct();

            // advices down to method
            IEnumerable<AdviceInfo> allAdvices = assemblyAndParents.SelectMany(a => a.GetAttributes<TAdvice>())
                .Union(GetTypeAndParentAdvices<TAdvice>(targetMethod.DeclaringType))
                .Union(targetMethod.GetAttributes<TAdvice>())
                .Select(CreateAdvice)
                .ToArray();

            // optional from property
            relatedPropertyInfo = GetPropertyInfo(targetMethod);
            if (relatedPropertyInfo != null)
                allAdvices = allAdvices.Union(relatedPropertyInfo.Item1.GetAttributes<TAdvice>().Select(CreateAdvice)).ToArray();

            // optional from event
            relatedEventInfo = GetEventInfo(targetMethod);
            if (relatedEventInfo != null)
                allAdvices = allAdvices.Union(relatedEventInfo.Item1.GetAttributes<TAdvice>().Select(CreateAdvice)).ToArray();

            // now separate parameters
            var parameterAdvices = allAdvices.Where(a => a.Advice is IParameterAdvice).ToArray();
            allAdvices = allAdvices.Where(a => !(a.Advice is IParameterAdvice)).ToArray();

            // and parameters (not union but concat, because same attribute may be applied at different levels)
            // ... indexed parameters
            var parameters = targetMethod.GetParameters();
            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var index = parameterIndex;
                allAdvices = allAdvices
                    .Concat(parameterAdvices.Select(p => CreateAdviceIndex(p.Advice, parameterIndex)))
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

        /// <summary>
        /// Gets advices applied to type and parent, regarding the <see cref="AttributeUsageAttribute.Inherited"/> flag for parent types.
        /// </summary>
        /// <typeparam name="TAdvice">The type of the advice.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static IEnumerable<TAdvice> GetTypeAndParentAdvices<TAdvice>(Type type)
            where TAdvice : class, IAdvice
        {
            bool typeItSelf = true;
            foreach (var typeAncestor in type.GetSelfAndAncestors())
            {
                var typeAttributes = typeAncestor.GetAttributes<TAdvice>();
                foreach (var typeAttribute in typeAttributes)
                {
                    if (typeItSelf || IsInheritable(typeAttribute))
                        yield return typeAttribute;
                }

                typeItSelf = false;
            }
        }

        /// <summary>
        /// Determines whether the specified attribute is inheritable.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>
        ///   <c>true</c> if the specified attribute is inheritable; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsInheritable(object attribute)
        {
            var attributeUsage = attribute.GetType().GetAttributes<AttributeUsageAttribute>().FirstOrDefault();
            if (attributeUsage == null)
                return true;
            return attributeUsage.Inherited;
        }

        internal static IEnumerable<AdviceInfo> GetAdvices<TAdvice>(MethodBase targetMethod,
            out Tuple<PropertyInfo, bool> relatedPropertyInfo, out Tuple<EventInfo, bool> relatedEventInfo)
            where TAdvice : class, IAdvice
        {
            // first of all, get all advices that should apply here
            var allAdvices = GetAllAdvices<TAdvice>(targetMethod, out relatedPropertyInfo, out relatedEventInfo);
            // then, keep only selected advices (by their declaring pointcuts)
            var selectedAdvices = allAdvices.Where(a => Select(targetMethod, a));
            // if method declares an exclusion, use it
            var adviceSelector = GetAdviceSelector(targetMethod);
            // remaining advices
            var advices = selectedAdvices.Where(a => SelectAdvice(a, adviceSelector));
            return advices;
        }

        private static bool SelectAdvice(AdviceInfo adviceInfo, PointcutSelector rules)
        {
            return rules.Select(adviceInfo.Advice.GetType().FullName, null, null);
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
            var propertyInfo = methodInfo.DeclaringType.GetMembersReader()
                .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                               BindingFlags.NonPublic)
                .SingleOrDefault(p => p.GetGetMethod(true) == methodInfo || p.GetSetMethod(true) == methodInfo);
            if (propertyInfo == null)
                return null; // this should never happen

            return Tuple.Create(propertyInfo, isSetter);
        }

        /// <summary>
        /// Gets the EventInfo, related to a method.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <returns>A tuple with the PropertyInfo and true is method is a setter (false for a getter)</returns>
        private static Tuple<EventInfo, bool> GetEventInfo(MemberInfo memberInfo)
        {
            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null || !methodInfo.IsSpecialName)
                return null;

            var isAdder = methodInfo.Name.StartsWith("add_");
            var isRemover = methodInfo.Name.StartsWith("remove_");
            if (!isAdder && !isRemover)
                return null;

            // now try to find the property
            // ReSharper disable once PossibleNullReferenceException
            var eventInfo = methodInfo.DeclaringType.GetMembersReader()
                .GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .SingleOrDefault(p => p.GetAddMethod(true) == methodInfo || p.GetRemoveMethod(true) == methodInfo);
            if (eventInfo == null)
                return null; // this should never happen

            return Tuple.Create(eventInfo, isAdder);
        }
    }
}