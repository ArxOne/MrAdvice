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
        public bool Weave(ModuleDefMD moduleDefinition)
        {
            var auditTimer = new AuditTimer();
            try
            {
                // sanity check
                auditTimer.NewZone("Types import");
                // context
                var context = CreateWeavingContext(moduleDefinition);
                if (context.AdviceInterfaceType == null)
                {
                    Logging.WriteWarning("IAdvice interface not found here (not referenced means not used), exiting");
                    return false;
                }
                // runtime check
                auditTimer.NewZone("Runtime check");
                var targetFramework = GetTargetFramework(moduleDefinition);
                InjectAsPrivate = targetFramework.Silverlight == null && targetFramework.WindowsPhone == null;

                // weave methods (they can be property-related, too)
                auditTimer.NewZone("Weavable methods detection");
                Func<MarkedNode, bool> isWeavable = n => !IsFromComputerGeneratedType(n) && IsWeavable(n);
                var weavingAdvicesMethods = GetMarkedMethods(moduleDefinition, context.WeavingAdviceInterfaceType, context).Where(isWeavable).ToArray();
                var weavableMethods = GetMarkedMethods(moduleDefinition, context.AdviceInterfaceType, context).Where(isWeavable).ToArray();
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
                weavableMethods.ForAll(m => WeaveMethod(moduleDefinition, m, context));

                auditTimer.NewZone("Weavable interfaces detection");
                var weavableInterfaces = GetAdviceHandledInterfaces(moduleDefinition).Union(GetDynamicHandledInterfaces(moduleDefinition)).ToArray();
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
                    moduleDefinition.Assembly.FullName, (int)report.Sum(r => r.Value.TotalMilliseconds), targetFramework.ToString(), Product.Version);

                return true;
            }
            catch (Exception e)
            {
                Logging.WriteError("Internal error during {0}: {1}", auditTimer.CurrentZoneName, e);
                Logging.WriteError("Please complain, whine, cry, yell at https://github.com/ArxOne/MrAdvice/issues/new");
                return false;
            }
        }

        private WeavingContext CreateWeavingContext(ModuleDef moduleDefinition)
        {
            var context = new WeavingContext
            {
                CompilerGeneratedAttributeType = moduleDefinition.Import(typeof(CompilerGeneratedAttribute)),
                PriorityAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(PriorityAttribute)),
                AbstractTargetAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(AbstractTargetAttribute)),
                AdviceInterfaceType = TypeResolver.Resolve(moduleDefinition, typeof(IAdvice)),
                WeavingAdviceInterfaceType = TypeResolver.Resolve(moduleDefinition, typeof(IWeavingAdvice)),
                ExecutionPointAttributeDefaultCtor = moduleDefinition.Import(TypeResolver.Resolve(moduleDefinition, typeof(ExecutionPointAttribute))?.FindDefaultConstructor()),
                ExcludePointcutAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(ExcludePointcutAttribute)),
                IncludePointcutAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(IncludePointcutAttribute)),
                ExcludeAdviceAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(ExcludeAdvicesAttribute)),
                IntroducedFieldType = TypeResolver.Resolve(moduleDefinition, typeof(IntroducedField<>)),
                SharedIntroducedFieldType = TypeResolver.Resolve(moduleDefinition, typeof(SharedIntroducedField<>)),
#pragma warning disable 618
                IntroducedFieldsType = TypeResolver.Resolve(moduleDefinition, typeof(IntroducedFieldsRegistry)),
#pragma warning restore 618
            };

            if (context.AdviceInterfaceType != null)
            {
                if (context.ExecutionPointAttributeDefaultCtor == null)
                    Logging.WriteError("ExecutionPointAttribute default ctor was not found");

                if (context.ExcludePointcutAttributeType == null)
                    Logging.WriteError("ExcludePointcutAttributeType was not found");
                if (context.IncludePointcutAttributeType == null)
                    Logging.WriteError("IncludePointcutAttributeType was not found");
                if (context.ExcludeAdviceAttributeType == null)
                    Logging.WriteError("ExcludeAdviceAttributeType was not found");
            }

            return context;
        }

        private static IEnumerable<FieldDef> GetRemovableFields(IList<MarkedNode> nodes, WeavingContext context)
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

        private static IEnumerable<Tuple<FieldDef, int>> GetRemovableFields(IEnumerable<MethodDef> methods, WeavingContext context)
        {
            var allFields = methods.SelectMany(m => GetRemovableFields(m, context));
            var fieldsCount = allFields.GroupBy(f => f);
            return fieldsCount.Select(f => Tuple.Create(f.Key, f.Count()));
        }

        private static IEnumerable<FieldDef> GetRemovableFields(MethodDef method, WeavingContext context)
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
                if (literalRuntimeVersion.StartsWith("v") && Version.TryParse(literalRuntimeVersion.Substring(1), out var runtimeVersion))
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
        /// <param name="isShared">if set to <c>true</c> the introduced field is shared among advices of the same type.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        ///   <c>true</c> if the specified advice member type reference is introduction; otherwise, <c>false</c>.
        /// </returns>
        private bool IsIntroduction(ITypeDefOrRef adviceMemberTypeReference, out ITypeDefOrRef introducedFieldType, out bool isShared, WeavingContext context)
        {
            introducedFieldType = null;
            isShared = false;
            var genericAdviceMemberTypeReference = adviceMemberTypeReference.TryGetGenericInstSig();
            if (genericAdviceMemberTypeReference == null)
                return false;

            var genericAdviceMemberTypeDefinition = TypeResolver.Resolve(genericAdviceMemberTypeReference.GenericType.TypeDefOrRef);
            if (genericAdviceMemberTypeDefinition == null) // in DEBUG or bogus cases, this may not be resolved. Whatever, this is not our field
                return false;

            if (!genericAdviceMemberTypeDefinition.ImplementsType(context.IntroducedFieldType, TypeResolver))
                return false;

            introducedFieldType = genericAdviceMemberTypeReference.GenericArguments[0].ToTypeDefOrRef();
            isShared = genericAdviceMemberTypeDefinition.ImplementsType(context.SharedIntroducedFieldType, TypeResolver);
            return true;
        }

        private IEnumerable<TypeDef> GetDynamicHandledInterfaces(ModuleDef moduleDefinition)
        {
            var dynamicHandleAttributeType = TypeResolver.Resolve(moduleDefinition, typeof(DynamicHandleAttribute));
            foreach (var interfaceDefinition in moduleDefinition.Types.Where(t => t.IsInterface))
            {
                if (interfaceDefinition.CustomAttributes.Any(c => c.AttributeType.SafeEquivalent(dynamicHandleAttributeType)))
                    yield return interfaceDefinition;
            }
        }

        /// <summary>
        /// Gets the advice handled interfaces.
        /// This is done by analyzing calls in all methods from module
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <returns></returns>
        private IEnumerable<TypeDef> GetAdviceHandledInterfaces(ModuleDef moduleDefinition)
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
                            Logging.WriteDebug("Now looking for references to '{0} [{1}]'", methodToSearch.Item1.ToString(), parameterIndex);
                        }
                    }
                    // only interfaces are processed by now
                    else
                    {
                        var interfaceDef = t.Item1 as TypeDef;
                        if (interfaceDef == null)
                        {
                            if (t.Item1 is TypeRef interfaceRef)
                                interfaceDef = TypeResolver.Resolve(interfaceRef);
                        }
                        if (interfaceDef == null)
                        {
                            Logging.WriteError("Can not identify {0} as valid weavable interface. If you feel this is unfair --> https://github.com/ArxOne/MrAdvice/issues/new", t.Item1.ToString());
                            continue;
                        }
                        if (interfaceDef.IsInterface)
                        {
                            // otherwise, this is a direct call, keep the injected interface name
                            if (!foundHandledInterfaces.Contains(t.Item1))
                            {
                                foundHandledInterfaces.Add(t.Item1);
                                yield return interfaceDef;
                            }
                        }
                        else
                            Logging.WriteError("Only interfaces can be weaved with Handle<>() extension method and {0} it not an interface (but I'm glad you asked)", interfaceDef.FullName);
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
        /// Indicates if the node belongs to a computer-generated type
        /// </summary>
        /// <param name="markedMethod">The marked method.</param>
        /// <returns>
        ///   <c>true</c> if [is from computer generated type] [the specified marked method]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsFromComputerGeneratedType(MarkedNode markedMethod)
        {
            var parentType = markedMethod.Node.GetSelfAndAncestors().OfType<TypeReflectionNode>().FirstOrDefault();
            if (parentType == null)
                return false;
            var isFromComputerGeneratedType = parentType.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
            if (isFromComputerGeneratedType)
                Logging.WriteDebug("Not weaving method '{0}' (from generated type)", markedMethod.Node.Method.FullName);
            return isFromComputerGeneratedType;
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
            var ancestorsToChildren = reflectionNode.GetAncestorsToDescendants().ToArray();
            return from node in ancestorsToChildren
                   where node.Method != null
                   let allMakersNode = new MarkedNode(node, GetAllMarkers(node, markerInterface, context).Select(t => t.Item2))
                   where allMakersNode.Definitions.Any() && IsIncludedByPointcut(allMakersNode, context) //&& !IsDeclaredByValue(node)
                   let includedMarkersNode = new MarkedNode(node, allMakersNode.Definitions.Where(d => IsIncludedByNode(d, node, context)))
                   where includedMarkersNode.Definitions.Any()
                   select includedMarkersNode;
        }

        /// <summary>
        /// Determines whether the <see cref="MarkedNode"/> is included by pointcut
        /// </summary>
        /// <param name="markedNode">The marked node.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        ///   <c>true</c> if [is included by pointcut] [the specified marked node]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsIncludedByPointcut(MarkedNode markedNode, WeavingContext context)
        {
            var isIncludedByPointcut = GetPointcutSelectors(markedNode, context).Any(s => s.Select(markedNode.Node.Method));
            if (!isIncludedByPointcut)
                Logging.WriteDebug("Excluding method '{0}' according to pointcut rules", markedNode.Node.Method.FullName);
            return isIncludedByPointcut;
        }

        /// <summary>
        /// Indicates whether the node belongs to a strut
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsDeclaredByValue(ReflectionNode node)
        {
            var ownerType = node.GetSelfAndAncestors().OfType<TypeReflectionNode>().First();
            // this should not happen
            if (ownerType is null)
                return false;
            return !ownerType.TypeDefinition.IsClass;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ReflectionNode"/> allows the given <see cref="MarkerDefinition"/>.
        /// </summary>
        /// <param name="markerDefinition">The marker definition.</param>
        /// <param name="node">The node.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        ///   <c>true</c> if [is included by node] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsIncludedByNode(MarkerDefinition markerDefinition, ReflectionNode node, WeavingContext context)
        {
            var adviceSelector = GetAdviceSelector(node, context);
            var isIncluded = adviceSelector.Select(markerDefinition.Type);
            if (!isIncluded)
                Logging.WriteDebug("Method '{0}' excluded advice '{1}'", node.Method.FullName, markerDefinition.Type.FullName);
            return isIncluded;
        }

        /// <summary>
        /// Gets all attributes that implement the given advice interface
        /// </summary>
        /// <param name="reflectionNode">The reflection node.</param>
        /// <param name="markerInterface">The advice interface.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private IEnumerable<Tuple<ReflectionNode, MarkerDefinition>> GetAllMarkers(ReflectionNode reflectionNode, ITypeDefOrRef markerInterface, WeavingContext context)
        {
            var markers = reflectionNode.GetAncestorsToDescendants()
                .Select(n => new { Node = n, Attributes = n.CustomAttributes })
                .SelectMany(n => n.Attributes.Select(a => new { Node = n.Node, Attribute = a })
                    .Where(a => !a.Attribute.AttributeType.DefinitionAssembly.IsSystem())
                    .Select(a => new { Node = a.Node, Type = ResolveTypeOrGenericDefinition(a.Attribute.AttributeType) })
                    .Where(t => IsMarker(t.Type, markerInterface)))
                .Select(t => Tuple.Create(t.Node, GetMarkerDefinition(t.Type, context)));
            return markers;
        }

        private TypeDef ResolveTypeOrGenericDefinition(ITypeDefOrRef typeDefOrRef)
        {
            if (typeDefOrRef is TypeDef typeDef)
                return typeDef;

            if (typeDefOrRef is TypeRef typeRef)
                return TypeResolver.Resolve(typeRef);

            // tricky part here: assuming this a generic type
            var typeSpec = (TypeSpec)typeDefOrRef;
            var genericType = typeSpec.TryGetGenericInstSig();
            return genericType?.GenericType.TypeDef;
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
                if (_markerDefinitions.TryGetValue(typeDefinition, out var markerDefinition))
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

#if uneedednow
            int priority = 0;
            if (priorityAttributes.Count > 0)
            {
                var priorityAttribute = priorityAttributes[0];
                priority = (int)priorityAttribute.ConstructorArguments[0].Value;
                Logging.WriteDebug("Advice {0} has priority {1}", typeDefinition.FullName, priority);
            }
#endif

            var abstractTarget = typeDefinition.CustomAttributes.Any(a => a.AttributeType.SafeEquivalent(context.AbstractTargetAttributeType));
            if (abstractTarget)
                Logging.WriteDebug("Advice {0} abstracts target", typeDefinition.FullName);
            var markerDefinition = new MarkerDefinition(typeDefinition, abstractTarget);
            return markerDefinition;
        }

        private readonly IDictionary<Tuple<string, string>, bool> _isMarker = new Dictionary<Tuple<string, string>, bool>();

        /// <summary>
        /// Determines whether the specified type reference implements directly or indirectly a requested marker interface.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <param name="markerInterface">The aspect marker interface.</param>
        /// <returns></returns>
        private bool IsMarker(ITypeDefOrRef typeReference, ITypeDefOrRef markerInterface)
        {
            if (typeReference is null)
                return false;
            lock (_isMarker)
            {
                var key = Tuple.Create(typeReference.FullName, markerInterface.FullName);
                // there is a cache, because the same attribute may be found several time
                // and we're in a hurry, the developper is waiting for his program to start!
                if (_isMarker.TryGetValue(key, out var isMarker))
                    return isMarker;

                // otherwise look for type or implemented interfaces (recursively)
                var typeDef = ResolveTypeOrGenericDefinition(typeReference);
                if (typeDef == null)
                    return false;
                var interfaces = typeDef.Interfaces;
                _isMarker[key] = isMarker = typeReference.SafeEquivalent(markerInterface)
                                            || interfaces.Any(i => IsMarker(i.Interface, markerInterface))
                                            || IsMarker(typeDef.BaseType, markerInterface);
                return isMarker;
            }
        }
    }
}
