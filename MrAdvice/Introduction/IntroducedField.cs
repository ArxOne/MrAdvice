#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Introduction
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Advice;

    /// <summary>
    /// Internal. Use <see cref="IntroducedField{TFieldType}"/>
    /// </summary>
    public abstract class IntroducedField
    {
        private static readonly object FieldInfosLock = new object();

        private readonly IAdvice _ownerAdvice;
        private readonly MemberInfo _ownerMemberInfo;

        /// <summary>
        /// Gets a value indicating whether the field is shared.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shared; otherwise, <c>false</c>.
        /// </value>
        protected abstract bool IsShared { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroducedField"/> class.
        /// </summary>
        protected IntroducedField(IAdvice ownerAdvice, MemberInfo ownerMemberInfo)
        {
            _ownerAdvice = ownerAdvice;
            _ownerMemberInfo = ownerMemberInfo;
        }

        /// <summary>
        /// Gets the field infos registry. It is shared per instance because of shared introduced fields
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
#pragma warning disable 618
        private IntroducedFieldsRegistry GetIntroducedFieldsRegistry(IAdviceContextTarget context)
        {
            var contextTargetType = context.TargetType;
            var registryField = GetRegistryField(contextTargetType);
            var registry = (IntroducedFieldsRegistry)registryField.GetValue(context.Target);
            if (registry is null)
            {
                lock (FieldInfosLock)
                {
                    registry = (IntroducedFieldsRegistry)registryField.GetValue(context.Target);
                    if (registry is null)
                    {
                        registry = new IntroducedFieldsRegistry();
                        registryField.SetValue(context.Target, registry);
                    }
                }
            }

            return registry;
        }

        private readonly IDictionary<Type, FieldInfo> _registryFields = new ConcurrentDictionary<Type, FieldInfo>();

        private FieldInfo GetRegistryField(Type contextTargetType)
        {
            if (!_registryFields.TryGetValue(contextTargetType, out var registryFieldInfo))
                _registryFields[contextTargetType] = registryFieldInfo = LoadRegistryField(contextTargetType);
            return registryFieldInfo;
        }

        private static FieldInfo LoadRegistryField(Type contextTargetType)
        {
            var registryField = contextTargetType.GetMembersReader().GetField(IntroductionRules.RegistryName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? contextTargetType.GetMembersReader().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.FieldType == typeof(IntroducedFieldsRegistry));
            return registryField;
        }

        /// <summary>
        /// Gets the introduced field.
        /// Since the attribute may be at assembly level, the advised type is given as parameter
        /// A cache is kept, by target type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected FieldInfo GetIntroducedField(IAdviceContextTarget context)
        {
            var targetType = context.TargetType;
            // this is where the advice is applied (method, property, event). For shared events this is instance-wide, so unrelated to any target
            var targetName = IsShared ? null : context.TargetName;
            var fieldInfos = GetIntroducedFieldsRegistry(context);
            lock (fieldInfos)
            {
                //var key = Tuple.Create(targetType, targetName);
                var key = new IntroducedFieldsRegistry.Key(_ownerAdvice.GetType(), _ownerMemberInfo, targetName);
                if (fieldInfos.Fields.TryGetValue(key, out var introducedField))
                    return introducedField;

                fieldInfos.Fields[key] = introducedField = Invocation.FindIntroducedField(_ownerAdvice, _ownerMemberInfo, targetType, targetName);
                return introducedField;
            }
        }
#pragma warning restore 618
    }

    /// <summary>
    /// This class allows to introduce fields in advised type
    /// To use it, declare instances of it in advice,
    /// then use the indexer to access introduced field in advised type instance
    /// This field (the default) introduces one field per advice. If you advise two methods in the same class, you'll get to introduced fields.
    /// In order to get one introduced field per class, use <see cref="SharedIntroducedField{TField}"/>
    /// </summary>
    /// <typeparam name="TFieldType">The type of the field type.</typeparam>
    public class IntroducedField<TFieldType> : IntroducedField
    {
        /// <summary>
        /// Gets or sets the <see typeparamref="TFieldType"/> with the specified context.
        /// </summary>
        /// <value>
        /// The <see typeparamref="TFieldType"/>.
        /// </value>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public TFieldType this[IAdviceContextTarget context]
        {
            get
            {
                var introducedField = GetIntroducedField(context);
                return (TFieldType)introducedField.GetValue(context.Target);
            }
            set
            {
                var introducedField = GetIntroducedField(context);
                introducedField.SetValue(context.Target, value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field is shared.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shared; otherwise, <c>false</c>.
        /// </value>
        protected override bool IsShared => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroducedField{TFieldType}" /> class.
        /// </summary>
        /// <param name="ownerAdvice">The owner advice.</param>
        /// <param name="ownerMemberInfo">The owner member information.</param>
        [Obsolete("Let Mr. Advice use it, don't bother instantiating it by yourself.")]
        // ReSharper disable once MemberCanBeProtected.Global
        public IntroducedField(IAdvice ownerAdvice, MemberInfo ownerMemberInfo)
            : base(ownerAdvice, ownerMemberInfo)
        {
        }
    }
}
