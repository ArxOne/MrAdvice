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
    using Introduction;
    using IO;
    using Mono.Cecil;
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
            var weavableMethods = GetMarkedMethods(new ModuleReflectionNode(moduleDefinition), adviceInterface).ToArray();
            auditTimer.NewZone("Methods weaving");
            weavableMethods.AsParallel().ForAll(m => WeaveMethod(moduleDefinition, m, adviceInterface));

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

            Logger.Write("MrAdvice weaved module '{0}' (targeting framework {2}) in {1}ms",
                moduleDefinition.Assembly.FullName, (int)stopwatch.ElapsedMilliseconds, targetFramework);
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
        /// Introduces the member.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="memberType">Type of the member.</param>
        /// <param name="isStatic">if set to <c>true</c> [is static].</param>
        /// <param name="adviceType">The advice.</param>
        /// <param name="advisedType">The type definition.</param>
        /// <param name="markerAttributeCtor">The marker attribute ctor.</param>
        private void IntroduceMember(ModuleDefinition moduleDefinition, string memberName, TypeReference memberType, bool isStatic,
            TypeReference adviceType, TypeDefinition advisedType, MethodReference markerAttributeCtor)
        {
            TypeReference introducedFieldType;
            if (IsIntroduction(moduleDefinition, memberType, out introducedFieldType))
            {
                var introducedFieldName = IntroductionRules.GetName(adviceType.Namespace, adviceType.Name, memberName);
                lock (advisedType.Fields)
                {
                    if (advisedType.Fields.All(f => f.Name != introducedFieldName))
                    {
                        var fieldAttributes = (InjectAsPrivate ? FieldAttributes.Private : FieldAttributes.Public) | FieldAttributes.NotSerialized;
                        if (isStatic)
                            fieldAttributes |= FieldAttributes.Static;
                        Logger.WriteDebug("Introduced file type '{0}'", introducedFieldType.FullName);
                        var introducedFieldTypeReference = moduleDefinition.SafeImport(introducedFieldType);
                        var introducedField = new FieldDefinition(introducedFieldName, fieldAttributes, introducedFieldTypeReference);
                        introducedField.CustomAttributes.Add(new CustomAttribute(markerAttributeCtor));
                        advisedType.Fields.Add(introducedField);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the advice member is introduction, based on its type.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="adviceMemberTypeReference">The type reference.</param>
        /// <param name="introducedFieldType">Type of the introduced field.</param>
        /// <returns></returns>
        private bool IsIntroduction(ModuleDefinition moduleDefinition, TypeReference adviceMemberTypeReference, out TypeReference introducedFieldType)
        {
            if (!adviceMemberTypeReference.IsGenericInstance)
            {
                introducedFieldType = null;
                return false;
            }
            var index = adviceMemberTypeReference.FullName.IndexOf('<');
            var genericTypeName = adviceMemberTypeReference.FullName.Substring(0, index);
            if (genericTypeName != Binding.IntroducedFieldTypeName)
            {
                introducedFieldType = null;
                return false;
            }

            var introducedFieldTypeName = adviceMemberTypeReference.FullName.Substring(index + 1, adviceMemberTypeReference.FullName.Length - index - 2);
            introducedFieldType = TypeResolver.Resolve(moduleDefinition, introducedFieldTypeName, false);
            if (introducedFieldType == null)
                Logger.WriteWarning("Type '{0}' not resolved!", introducedFieldTypeName);
            return true;
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
                .SelectMany(n => n.CustomAttributes.Select(a => a.AttributeType).Where(t => IsMarker(t, markerInterface)))
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
