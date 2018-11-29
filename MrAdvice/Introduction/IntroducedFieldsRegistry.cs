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

    /// <summary>
    /// Fields registry
    /// </summary>
    [Obsolete("Internal use only, don't play with it")]
    public class IntroducedFieldsRegistry
    {
        /// <summary>
        /// Key for introduced fields mapping
        /// </summary>
        public class Key : IEquatable<Key>
        {
            private readonly Type _ownerAdviceType;
            private readonly MemberInfo _ownerMemberInfo;
            private readonly string _advisedTargetName;

            /// <summary>
            /// Initializes a new instance of the <see cref="IntroducedFieldsRegistry.Key"/> class.
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

        internal readonly IDictionary<Key, FieldInfo> Fields = new Dictionary<Key, FieldInfo>();
    }
}
