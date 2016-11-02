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
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using Advice;
    using Annotation;
    using dnlib.DotNet;
    using dnlib.DotNet.Emit;
    using Introduction;
    using Properties;
    using Reflection;
    using Reflection.Groups;
    using StitcherBoy.Logging;
    using Utility;
    using ModuleDefMD = dnlib.DotNet.ModuleDefMD;

    /// <summary>
    /// Aspect weaver core
    /// Pointcuts are identified here
    /// Advices are injected here
    /// </summary>
    internal partial class AspectWeaver
    {
        public TypeResolver TypeResolver { get; set; }

        public TypeLoader TypeLoader { get; set; }

        public ILogging Logging { get; set; }

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
        public void Weave(ModuleDefMD moduleDefinition)
        {
            var auditTimer = new AuditTimer();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // sanity check
            auditTimer.NewZone("IAdvice location");
            var adviceInterface = TypeResolver.Resolve(moduleDefinition, typeof(IAdvice));
            if (adviceInterface == null)
            {
                Logging.WriteWarning("IAdvice interface not found here (not referenced means not used), exiting");
                return;
            }

            // context
            var context = new WeavingContext
            {
                CompilerGeneratedAttributeType = moduleDefinition.Import(typeof(CompilerGeneratedAttribute)),
                PriorityAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(PriorityAttribute)),
                AbstractTargetAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(AbstractTargetAttribute)),
                WeavingAdviceAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(IWeavingAdvice))
            };
            // runtime check
            auditTimer.NewZone("Runtime check");
            var targetFramework = GetTargetFramework(moduleDefinition);
            InjectAsPrivate = targetFramework.Silverlight == null && targetFramework.WindowsPhone == null;

            // weave methods (they can be property-related, too)
            auditTimer.NewZone("Weavable methods detection");
            var weavingAdvicesMethods = GetMarkedMethods(moduleDefinition, context.WeavingAdviceAttributeType, context).Where(IsWeavable).ToArray();
            var weavableMethods = GetMarkedMethods(moduleDefinition, adviceInterface, context).Where(IsWeavable).ToArray();
            auditTimer.NewZone("Abstract targets");
            var generatedFieldsToBeRemoved = new List<FieldDef>();
            var methodsWithAbstractTarget = weavableMethods.Where(m => m.AbstractTarget).ToArray();
            if (methodsWithAbstractTarget.Length > 0)
            {
                generatedFieldsToBeRemoved.AddRange(GetRemovableFields(methodsWithAbstractTarget, context));
                foreach (var fieldReference in generatedFieldsToBeRemoved)
                    Logging.WriteDebug("Field {0} to be removed", fieldReference.FullName);
            }
            auditTimer.NewZone("Methods weaving advice");
            weavingAdvicesMethods.ForAll(i => RunWeavingAdvices(i, context));
            auditTimer.NewZone("Methods weaving");
            weavableMethods.ForAll(m => WeaveMethod(moduleDefinition, m, adviceInterface, context));

            auditTimer.NewZone("Weavable interfaces detection");
            var weavableInterfaces = GetAdviceHandledInterfaces(moduleDefinition).ToArray();
            auditTimer.NewZone("Interface methods weaving");
            weavableInterfaces.ForAll(i => WeaveInterface(moduleDefinition, i, context));

            // and then, the info advices
            auditTimer.NewZone("Info advices weaving");
            var infoAdviceInterface = TypeResolver.Resolve(moduleDefinition, typeof(IInfoAdvice));
            moduleDefinition.GetTypes().ForAll(t => WeaveInfoAdvices(moduleDefinition, t, infoAdviceInterface, context));

            auditTimer.NewZone("Abstract targets cleanup");
            foreach (var generatedFieldToBeRemoved in generatedFieldsToBeRemoved)
                generatedFieldToBeRemoved.DeclaringType.Fields.Remove(generatedFieldToBeRemoved);

            auditTimer.LastZone();

            var report = auditTimer.GetReport();
            var maxLength = report.Keys.Max(k => k.Length);
            Logging.WriteDebug("--- Timings --------------------------");
            foreach (var reportPart in report)
                Logging.WriteDebug("{0} : {1}ms", reportPart.Key.PadRight(maxLength), (int)reportPart.Value.TotalMilliseconds);
            Logging.WriteDebug("--------------------------------------");

            Logging.Write("MrAdvice {3} weaved module '{0}' (targeting framework {2}) in {1}ms",
                moduleDefinition.Assembly.FullName, (int)stopwatch.ElapsedMilliseconds, targetFramework.ToString(), Product.Version);
        }

        private IEnumerable<FieldDef> GetRemovableFields(IList<MarkedNode> nodes, WeavingContext context)
        {
            var type = nodes.First().Node.Method.DeclaringType;
            // get all types
            var allRemovableFields = GetRemovableFields(type.Methods, context);
            // then all referenced fields that will be dereferenced
            var toBeRemovedFields = GetRemovableFields(nodes.Select(n => n.Node.Method), context);
            // and remove only where count are equals
            var removableFields = from t in toBeRemovedFields
                                  where allRemovableFields.Contains(t)
                                  select t.Item1;
            return removableFields;
        }

        private IEnumerable<Tuple<FieldDef, int>> GetRemovableFields(IEnumerable<MethodDef> methods, WeavingContext context)
        {
            var allFields = methods.SelectMany(m => GetRemovableFields(m, context));
            var fieldsCount = allFields.GroupBy(f => f);
            return fieldsCount.Select(f => Tuple.Create(f.Key, f.Count()));
        }

        private IEnumerable<FieldDef> GetRemovableFields(MethodDef method, WeavingContext context)
        {
            return from instruction in method.Body.Instructions
                   let fieldReference = instruction.Operand as IField
                   where fieldReference != null && fieldReference.DeclaringType.SafeEquivalent(method.DeclaringType)
                   let fieldDefinition = fieldReference.ResolveFieldDef()
                   where fieldDefinition.CustomAttributes.Any(a => a.AttributeType.SafeEquivalent(context.CompilerGeneratedAttributeType))
                   select fieldDefinition;
        }

        /// <summary>
        /// Gets the target framework.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        private TargetFramework GetTargetFramework(ModuleDef moduleDefinition)
        {
            var targetFrameworkAttributeType = moduleDefinition.SafeImport(typeof(TargetFrameworkAttribute));
            var targetFrameworkAttribute = moduleDefinition.Assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.SafeEquivalent(targetFrameworkAttributeType));
            if (targetFrameworkAttribute == null)
            {
                var literalRuntimeVersion = moduleDefinition.RuntimeVersion;
                Version runtimeVersion;
                if (literalRuntimeVersion.StartsWith("v") && Version.TryParse(literalRuntimeVersion.Substring(1), out runtimeVersion))
                    return new TargetFramework(runtimeVersion);

                Logging.WriteError($"Unknown RuntimeVersion: '{literalRuntimeVersion}'");
                throw new ArgumentOutOfRangeException(literalRuntimeVersion, nameof(literalRuntimeVersion));
            }

            return new TargetFramework(((UTF8String)targetFrameworkAttribute.ConstructorArguments[0].Value).String);
        }

        /// <summary>
        /// Determines whether the advice member is introduction, based on its type.
        /// </summary>
        /// <param name="adviceMemberTypeReference">The type reference.</param>
        /// <param name="introducedFieldType">Type of the introduced field.</param>
        /// <returns></returns>
        private static bool IsIntroduction(ITypeDefOrRef adviceMemberTypeReference, out ITypeDefOrRef introducedFieldType)
        {
            var genericAdviceMemberTypeReference = adviceMemberTypeReference.TryGetGenericInstSig();
            if (genericAdviceMemberTypeReference == null || genericAdviceMemberTypeReference.GenericType.FullName != typeof(IntroducedField<>).FullName)
            {
                introducedFieldType = null;
                return false;
            }

            introducedFieldType = genericAdviceMemberTypeReference.GenericArguments[0].ToTypeDefOrRef();
            return true;
        }

        /// <summary>
        /// Gets the advice handled interfaces.
        /// This is done by analyzing calls in all methods from module
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        private IEnumerable<ITypeDefOrRef> GetAdviceHandledInterfaces(ModuleDef moduleDefinition)
        {
            // the first method to look for in the final AdviceExtensions.Handle<>() method
            var adviceExtensionsType = TypeResolver.Resolve(moduleDefinition, typeof(AdviceExtensions));
            var adviceHandleMethod = adviceExtensionsType.Methods.Single(m => m.IsPublic && m.HasGenericParameters && m.Name == nameof(AdviceExtensions.Handle));
            var methodsSearched = new HashSet<MethodDef>(new MethodReferenceComparer()) { adviceHandleMethod };
            var foundHandledInterfaces = new HashSet<ITypeDefOrRef>(new TypeReferenceComparer());
            var methodsToSearch = new List<Tuple<MethodDef, int>> { Tuple.Create(adviceHandleMethod, 0) };
            while (methodsToSearch.Count > 0)
            {
                var methodToSearch = methodsToSearch[0];
                methodsToSearch.RemoveAt(0);
                foreach (var t in GetAdviceHandledInterfaces(moduleDefinition, methodToSearch.Item1, methodToSearch.Item2))
                {
                    // if the supposed interface type itself is a generic parameter
                    // this means that the calling method (Item2) is itself a generic parameter
                    // and we have to lookup for calls to this method
                    if (t.Item1.ContainsGenericParameter)
                    {
                        if (!methodsSearched.Contains(t.Item2))
                        {
                            // ReSharper disable once AccessToForEachVariableInClosure
                            var parameterIndex = t.Item2.GenericParameters.IndexOf(p => p.Name == t.Item1.TypeName);
                            methodsSearched.Add(t.Item2);
                            methodsToSearch.Add(Tuple.Create(t.Item2, parameterIndex));
                            Logging.WriteDebug("Now looking for references to '{0} [{1}]'", methodToSearch, parameterIndex);
                        }
                    }
                    // only interfaces are processed by now
                    else if (TypeResolver.Resolve(t.Item1).IsInterface)
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
        private static IEnumerable<Tuple<ITypeDefOrRef, MethodDef>> GetAdviceHandledInterfaces(ModuleDef moduleDefinition,
            IMethodDefOrRef invokedMethod, int genericParameterIndex)
        {
            return moduleDefinition.GetTypes().SelectMany(t => t.Methods.Where(m => m.HasBody)
                .SelectMany(definition => GetAdviceHandledInterfaces(definition, invokedMethod, genericParameterIndex)));
        }

        /// <summary>
        /// Gets the advice handled interfaces.
        /// This is done by analyzing method body
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <param name="invokedMethod">The invoked method.</param>
        /// <param name="genericParameterIndex">Index of the generic parameter.</param>
        /// <returns></returns>
        private static IEnumerable<Tuple<ITypeDefOrRef, MethodDef>> GetAdviceHandledInterfaces(MethodDef methodDefinition, IMethodDefOrRef invokedMethod, int genericParameterIndex)
        {
            foreach (var instruction in methodDefinition.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Calli || instruction.OpCode == OpCodes.Callvirt)
                {
                    var invokedMethodReference = (IMethod)instruction.Operand;
                    if (invokedMethodReference.NumberOfGenericParameters > 0 && invokedMethodReference.SafeEquivalent(invokedMethod))
                    {
                        var methodSpec = (MethodSpec)invokedMethodReference;
                        var advisedInterface = methodSpec.GenericInstMethodSig.GenericArguments[genericParameterIndex];
                        //Logger.WriteDebug("Found Advice to '{0}'", advisedInterface);
                        yield return Tuple.Create(advisedInterface.ToTypeDefOrRef(), methodDefinition);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified method is weavable.
        /// </summary>
        /// <param name="markedMethod">The marked method.</param>
        /// <returns></returns>
        private static bool IsWeavable(MarkedNode markedMethod)
        {
            return markedMethod.Node.Method.HasBody || markedMethod.Node.Method.IsPinvokeImpl;
        }

        /// <summary>
        /// Gets the marked methods.
        /// </summary>
        /// <param name="reflectionNode">The reflection node.</param>
        /// <param name="markerInterface">The marker interface.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private IEnumerable<MarkedNode> GetMarkedMethods(ReflectionNode reflectionNode, ITypeDefOrRef markerInterface, WeavingContext context)
        {
            var ancestorsToChildren = reflectionNode.GetAncestorsToChildren().ToArray();
            return ancestorsToChildren
                .Where(n => n.Method != null)
                .Select(n => new MarkedNode { Node = n, Definitions = GetAllMarkers(n, markerInterface, context).ToArray() })
                .Where(m => m.Definitions.Length > 0);
        }

        /// <summary>
        /// Gets all attributes that implement the given advice interface
        /// </summary>
        /// <param name="reflectionNode">The reflection node.</param>
        /// <param name="markerInterface">The advice interface.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private IEnumerable<MarkerDefinition> GetAllMarkers(ReflectionNode reflectionNode, ITypeDefOrRef markerInterface, WeavingContext context)
        {
            var markers = reflectionNode.GetAncestorsToChildren()
                .SelectMany(n => n.CustomAttributes
                    .Where(a => !a.AttributeType.DefinitionAssembly.IsSystem())
                    .SelectMany(a => TypeResolver.Resolve(a.AttributeType).GetSelfAndParents())
                    .Where(t => IsMarker(t, markerInterface)))
                .Distinct()
                .Select(t => GetMarkerDefinition(t, context));
            return markers;
        }

        private readonly IDictionary<TypeDef, MarkerDefinition> _markerDefinitions = new Dictionary<TypeDef, MarkerDefinition>();

        /// <summary>
        /// Gets the marker definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private MarkerDefinition GetMarkerDefinition(TypeDef typeDefinition, WeavingContext context)
        {
            lock (_markerDefinitions)
            {
                MarkerDefinition markerDefinition;
                if (_markerDefinitions.TryGetValue(typeDefinition, out markerDefinition))
                    return markerDefinition;

                markerDefinition = CreateMarkerDefinition(typeDefinition, context);
                _markerDefinitions[typeDefinition] = markerDefinition;
                return markerDefinition;
            }
        }

        /// <summary>
        /// Creates the marker definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private MarkerDefinition CreateMarkerDefinition(TypeDef typeDefinition, WeavingContext context)
        {
            var priorityAttributes = typeDefinition.CustomAttributes.Where(a => a.AttributeType.SafeEquivalent(context.PriorityAttributeType)).ToList();
            if (priorityAttributes.Count > 1)
                Logging.WriteWarning("Advice {0} has more than one priority. Using the first found", typeDefinition.FullName);

            int priority = 0;
            if (priorityAttributes.Count > 0)
            {
                var b = priorityAttributes[0].GetBlob();
                priority = (b[5] << 24) | (b[4] << 16) | (b[3] << 8) | b[2];
                Logging.WriteDebug("Advice {0} has priority {1}", typeDefinition.FullName, priority);
            }

            var abstractTarget = typeDefinition.CustomAttributes.Any(a => a.AttributeType.SafeEquivalent(context.AbstractTargetAttributeType));
            if (abstractTarget)
                Logging.WriteDebug("Advice {0} abstracts target", typeDefinition.FullName, priority);
            var markerDefinition = new MarkerDefinition { Type = typeDefinition, Priority = priority, AbstractTarget = abstractTarget };
            return markerDefinition;
        }

        private readonly IDictionary<Tuple<string, string>, bool> _isMarker = new Dictionary<Tuple<string, string>, bool>();

        /// <summary>
        /// Determines whether the specified type reference is aspect.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="markerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private bool IsMarker(ITypeDefOrRef typeReference, ITypeDefOrRef markerInterface)
        {
            lock (_isMarker)
            {
                var key = Tuple.Create(typeReference.FullName, markerInterface.FullName);
                // there is a cache, because the same attribute may be found several time
                // and we're in a hurry, the developper is waiting for his program to start!
                bool isMarker;
                if (_isMarker.TryGetValue(key, out isMarker))
                    return isMarker;

                // otherwise look for type or implemented interfaces (recursively)
                var typeDef = TypeResolver.Resolve(typeReference);
                if (typeDef == null)
                    return false;
                var interfaces = typeDef.Interfaces;
                _isMarker[key] = isMarker = typeReference.SafeEquivalent(markerInterface)
                                               || interfaces.Any(i => IsMarker(i.Interface, markerInterface));
                return isMarker;
            }
        }
    }
}
