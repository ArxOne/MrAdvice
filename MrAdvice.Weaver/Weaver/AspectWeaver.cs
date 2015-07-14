#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Versioning;
    using IO;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;
    using Properties;
    using Reflection;
    using Reflection.Groups;
    using Utility;

    /// <summary>
    /// Aspect weaver core
    /// Pointcuts are identified here
    /// Advices are injected here
    /// </summary>
    internal partial class AspectWeaver
    {
        public TypeResolver TypeResolver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all additional methods and fields are injected as prived.
        /// Just because Silverlight does not like invoking private methods or fields, even by reflection
        /// </summary>
        /// <value>
        ///   <c>true</c> if [inject as private]; otherwise, <c>false</c>.
        /// </value>
        public bool InjectAsPrivate { get; set; }

        /// <summary>
        /// Weaves the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        public void Weave(ModuleDefinition moduleDefinition)
        {
            var auditTimer = new AuditTimer();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // sanity check
            auditTimer.NewZone("IAdvice location");
            var adviceInterface = TypeResolver.Resolve(moduleDefinition, Binding.AdviceInterfaceName, true);
            if (adviceInterface == null)
            {
                Logger.WriteWarning("IAdvice interface not found here, exiting");
                return;
            }

            // runtime check
            auditTimer.NewZone("Runtime check");
            var targetFramework = GetTargetFramework(moduleDefinition);
            InjectAsPrivate = targetFramework.Silverlight == null && targetFramework.WindowsPhone == null;

            //Logger.WriteDebug("t1: {0}ms", (int)stopwatch.ElapsedMilliseconds);

            // weave methods (they can be property-related, too)
            auditTimer.NewZone("Weavable methods detection");
            var weavableMethods = GetMarkedMethods(moduleDefinition, adviceInterface).Where(IsWeavable).ToArray();
            auditTimer.NewZone("Methods weaving");
            weavableMethods.AsParallel().ForAll(m => WeaveMethod(moduleDefinition, m, adviceInterface));

            auditTimer.NewZone("Weavable interfaces detection");
            var weavableInterfaces = GetAdviceHandledInterfaces(moduleDefinition).ToArray();
            auditTimer.NewZone("Interface methods weaving");
            weavableInterfaces.AsParallel().ForAll(i => WeaveInterface(moduleDefinition, i));

            //Logger.WriteDebug("t2: {0}ms", (int)stopwatch.ElapsedMilliseconds);

            // and then, the info advices
            auditTimer.NewZone("Info advices weaving");
            var infoAdviceInterface = TypeResolver.Resolve(moduleDefinition, Binding.InfoAdviceInterfaceName, true);
            moduleDefinition.GetTypes().AsParallel().ForAll(t => WeaveInfoAdvices(moduleDefinition, t, infoAdviceInterface));

            auditTimer.LastZone();

            //Logger.WriteDebug("t3: {0}ms", (int)stopwatch.ElapsedMilliseconds);

            var report = auditTimer.GetReport();
            var maxLength = report.Keys.Max(k => k.Length);
            Logger.WriteDebug("--- Timings --------------------------");
            foreach (var reportPart in report)
                Logger.WriteDebug("{0} : {1}ms", reportPart.Key.PadRight(maxLength), (int)reportPart.Value.TotalMilliseconds);
            Logger.WriteDebug("--------------------------------------");

            Logger.Write("MrAdvice {3} weaved module '{0}' (targeting framework {2}) in {1}ms",
                moduleDefinition.Assembly.FullName, (int)stopwatch.ElapsedMilliseconds, targetFramework, Product.Version);
        }

        /// <summary>
        /// Gets the target framework.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private static TargetFramework GetTargetFramework(ModuleDefinition moduleDefinition)
        {
            var targetFrameworkAttributeType = moduleDefinition.SafeImport(typeof(TargetFrameworkAttribute));
            var targetFrameworkAttribute = moduleDefinition.Assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.SafeEquivalent(targetFrameworkAttributeType));
            if (targetFrameworkAttribute == null)
            {
                switch (moduleDefinition.Runtime)
                {
                    case TargetRuntime.Net_1_0:
                        return new TargetFramework(new Version(1, 0));
                    case TargetRuntime.Net_1_1:
                        return new TargetFramework(new Version(1, 1));
                    case TargetRuntime.Net_2_0:
                        return new TargetFramework(new Version(2, 0));
                    case TargetRuntime.Net_4_0:
                        return new TargetFramework(new Version(4, 0));
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new TargetFramework((string)targetFrameworkAttribute.ConstructorArguments[0].Value);
        }

        /// <summary>
        /// Determines whether the advice member is introduction, based on its type.
        /// </summary>
        /// <param name="adviceMemberTypeReference">The type reference.</param>
        /// <param name="introducedFieldType">Type of the introduced field.</param>
        /// <returns></returns>
        private static bool IsIntroduction(TypeReference adviceMemberTypeReference, out TypeReference introducedFieldType)
        {
            if (!adviceMemberTypeReference.IsGenericInstance)
            {
                introducedFieldType = null;
                return false;
            }

            var genericAdviceMemberTypeReference = (GenericInstanceType)adviceMemberTypeReference;
            if (genericAdviceMemberTypeReference.GetElementType().FullName != Binding.IntroducedFieldTypeName)
            {
                introducedFieldType = null;
                return false;
            }

            introducedFieldType = genericAdviceMemberTypeReference.GenericArguments[0];
            return true;
        }

        /// <summary>
        /// Gets the advice handled interfaces.
        /// This is done by analyzing calls in all methods from module
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        private IEnumerable<TypeReference> GetAdviceHandledInterfaces(ModuleDefinition moduleDefinition)
        {
            // the first method to look for in the final AdviceExtensions.Handle<>() method
            var adviceExtensionsType = TypeResolver.Resolve(moduleDefinition, Binding.AdviceExtensionsTypeName, true);
            var adviceHandleMethod = adviceExtensionsType.GetMethods().Single(m => m.IsPublic && m.HasGenericParameters && m.Name == Binding.AdviceHandleMethodName);
            var methodsSearched = new HashSet<MethodReference>(new MethodReferenceComparer()) { adviceHandleMethod };
            var foundHandledInterfaces = new HashSet<TypeReference>(new TypeReferenceComparer());
            var methodsToSearch = new List<Tuple<MethodDefinition, int>> { Tuple.Create(adviceHandleMethod, 0) };
            while (methodsToSearch.Count > 0)
            {
                var methodToSearch = methodsToSearch[0];
                methodsToSearch.RemoveAt(0);
                foreach (var t in GetAdviceHandledInterfaces(moduleDefinition, methodToSearch.Item1, methodToSearch.Item2))
                {
                    // if the supposed interface type itself is a generic parameter
                    // this means that the calling method (Item2) is itself a generic parameter
                    // and we have to lookup for calls to this method
                    if (t.Item1.IsGenericParameter)
                    {
                        if (!methodsSearched.Contains(t.Item2))
                        {
                            // ReSharper disable once AccessToForEachVariableInClosure
                            var parameterIndex = t.Item2.GenericParameters.IndexOf(p => p.Name == t.Item1.Name);
                            methodsSearched.Add(t.Item2);
                            methodsToSearch.Add(Tuple.Create(t.Item2, parameterIndex));
                            Logger.WriteDebug("Now looking for references to '{0} [{1}]'", methodToSearch, parameterIndex);
                        }
                    }
                    // only interfaces are processed by now
                    else if (t.Item1.Resolve().IsInterface)
                    {
                        // otherwise, this is a direct call, keep the injected interface name
                        if (!foundHandledInterfaces.Contains(t.Item1))
                        {
                            foundHandledInterfaces.Add(t.Item1);
                            yield return t.Item1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the advice handled interfaces.
        /// This is done by analyzing calls in all methods from module
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="invokedMethod">The invoked method.</param>
        /// <param name="genericParameterIndex">Index of the generic parameter.</param>
        /// <returns></returns>
        private static IEnumerable<Tuple<TypeReference, MethodDefinition>> GetAdviceHandledInterfaces(ModuleDefinition moduleDefinition,
            MethodReference invokedMethod, int genericParameterIndex)
        {
            return moduleDefinition.GetTypes().SelectMany(t => t.GetMethods().Where(m => m.HasBody)
                .AsParallel().SelectMany(definition => GetAdviceHandledInterfaces(definition, invokedMethod, genericParameterIndex)));
        }

        /// <summary>
        /// Gets the advice handled interfaces.
        /// This is done by analyzing method body
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <param name="invokedMethod">The invoked method.</param>
        /// <param name="genericParameterIndex">Index of the generic parameter.</param>
        /// <returns></returns>
        private static IEnumerable<Tuple<TypeReference, MethodDefinition>> GetAdviceHandledInterfaces(MethodDefinition methodDefinition,
            MethodReference invokedMethod, int genericParameterIndex)
        {
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Calli || instruction.OpCode == OpCodes.Callvirt)
                {
                    var invokedMethodReference = (MethodReference)instruction.Operand;
                    if (invokedMethodReference.IsGenericInstance && invokedMethodReference.SafeEquivalent(invokedMethod))
                    {
                        var advisedInterface = ((GenericInstanceMethod)invokedMethodReference).GenericArguments[genericParameterIndex];
                        //Logger.WriteDebug("Found Advice to '{0}'", advisedInterface);
                        yield return Tuple.Create(advisedInterface, methodDefinition);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified method is weavable.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <returns></returns>
        private static bool IsWeavable(MethodDefinition methodDefinition)
        {
            return !methodDefinition.IsAbstract;
        }

        /// <summary>
        /// Gets the marked methods.
        /// </summary>
        /// <param name="reflectionNode">The reflection node.</param>
        /// <param name="markerInterface">The marker interface.</param>
        /// <returns></returns>
        private IEnumerable<MethodDefinition> GetMarkedMethods(ReflectionNode reflectionNode, TypeDefinition markerInterface)
        {
            return reflectionNode.GetAncestorsToChildren().AsParallel()
                .Where(n => n.Method != null && GetAllMarkers(n, markerInterface).Any())
                .Select(n => n.Method);
        }

        /// <summary>
        /// Gets all attributes that implement the given advice interface
        /// </summary>
        /// <param name="reflectionNode">The reflection node.</param>
        /// <param name="markerInterface">The advice interface.</param>
        /// <returns></returns>
        private IEnumerable<TypeReference> GetAllMarkers(ReflectionNode reflectionNode, TypeDefinition markerInterface)
        {
            var markers = reflectionNode.GetAncestorsToChildren()
                .SelectMany(n => n.CustomAttributes.SelectMany(a => a.AttributeType.Resolve().GetSelfAndParents()).Where(t => IsMarker(t, markerInterface)))
                .Distinct();
#if DEBUG
            //            Logger.WriteDebug(string.Format("{0} --> {1}", reflectionNode.ToString(), markers.Count()));
#endif
            return markers;
        }

        private readonly IDictionary<Tuple<string, string>, bool> _isInterface = new Dictionary<Tuple<string, string>, bool>();

        /// <summary>
        /// Determines whether the specified type reference is aspect.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="markerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private bool IsMarker(TypeReference typeReference, TypeDefinition markerInterface)
        {
            lock (_isInterface)
            {
                var key = Tuple.Create(typeReference.FullName, markerInterface.FullName);
                // there is a cache, because the same attribute may be found several time
                // and we're in a hurry, the developper is waiting for his program to start!
                bool isMarker;
                if (_isInterface.TryGetValue(key, out isMarker))
                    return isMarker;

                // otherwise look for type or implemented interfaces (recursively)
                var interfaces = typeReference.Resolve().Interfaces;
                _isInterface[key] = isMarker = typeReference.SafeEquivalent(markerInterface)
                                               || interfaces.Any(i => IsMarker(i, markerInterface));
                return isMarker;
            }
        }
    }
}
