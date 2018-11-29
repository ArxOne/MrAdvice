#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Introduction
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Advice;

    /// <summary>
    /// Internal. Use <see cref="IntroducedField{TFieldType}"/>
    /// </summary>
    public abstract class IntroducedField
    {
        /// <summary>
        /// Key for introduced fields mapping
        /// </summary>
        protected class Key : IEquatable<Key>
        {
            private readonly Type _ownerAdviceType;
            private readonly MemberInfo _ownerMemberInfo;
            private readonly string _advisedTargetName;

            /// <summary>
            /// Initializes a new instance of the <see cref="Key"/> class.
            /// </summary>
            /// <param name="ownerAdviceType"></param>
            /// <param name="ownerMemberInfo">The owner member information.</param>
            /// <param name="advisedTargetName">Name of the advised target.</param>
            public Key(Type ownerAdviceType, MemberInfo ownerMemberInfo, string advisedTargetName)
            {
                _ownerAdviceType = ownerAdviceType;
                _ownerMemberInfo = ownerMemberInfo;
                _advisedTargetName = advisedTargetName;
            }

            /// <inheritdoc />
            public bool Equals(Key other)
            {
                return ReferenceEquals(_ownerAdviceType, other._ownerAdviceType)
                       && Equals(_ownerMemberInfo, other._ownerMemberInfo)
                       && _advisedTargetName == other._advisedTargetName;
            }

            /// <inheritdoc />
            public override bool Equals(object obj) => Equals(obj as Key);

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return _ownerAdviceType.GetHashCode() ^ _ownerMemberInfo.GetHashCode() ^ (_advisedTargetName ?? "").GetHashCode();
            }
        }

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
        protected static IDictionary<Key, FieldInfo> GetIntroducedFieldsRegistry(IAdviceContextTarget context)
        {
            var registryField = context.TargetType.GetMembersReader().GetField(IntroductionRules.RegistryName, BindingFlags.Instance | BindingFlags.NonPublic);
            var registry = (IDictionary<Key, FieldInfo>)registryField.GetValue(context.Target);
            if (registry is null)
            {
                lock (FieldInfosLock)
                {
                    registry = (IDictionary<Key, FieldInfo>)registryField.GetValue(context.Target);
                    if (registry is null)
                    {
                        registry = new Dictionary<Key, FieldInfo>();
                        registryField.SetValue(context.Target, registry);
                    }
                }
            }

            return registry;
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
                var key = new Key(_ownerAdvice.GetType(), _ownerMemberInfo, targetName);
                if (fieldInfos.TryGetValue(key, out var introducedField))
                    return introducedField;

                fieldInfos[key] = introducedField = Invocation.FindIntroducedField(_ownerAdvice, _ownerMemberInfo, targetType, targetName);
                return introducedField;
            }
        }
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
