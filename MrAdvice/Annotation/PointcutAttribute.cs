#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using System;

    /// <summary>
    /// Allows to include or exclude namespaces/types/methods/etc. from being advised
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class PointcutAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name matching patterns.
        /// Default is extended Wildcard, Regex mode is enabled by using ^ at start or $ at end
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string[] Names { get; set; }

        /// <summary>
        /// Gets or sets the attributes to match.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public MemberAttributes Attributes { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PointcutAttribute"/> is include.
        /// </summary>
        /// <value>
        ///   <c>true</c> if include; otherwise, <c>false</c>.
        /// </value>
        public abstract bool Include { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointcutAttribute"/> class.
        /// </summary>
        protected PointcutAttribute()
            : this(new string[0])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointcutAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        protected PointcutAttribute(params string[] names)
        {
            Names = names;
        }
    }

    /// <summary>
    /// Pointcut member matching visibility
    /// </summary>
    [Flags]
    public enum MemberAttributes
    {
        /// <summary>
        /// Public type
        /// </summary>
        PublicType = 0x01,
        /// <summary>
        /// Public member
        /// </summary>
        PublicMember = 0x02,
        /// <summary>
        /// Anything public
        /// </summary>
        Public = PublicType | PublicMember,

        /// <summary>
        /// Member is accessible only from type and inherited types
        /// Yes, that's "protected"
        /// </summary>
        FamilyMember = 0x04,
        /// <summary>
        /// Type is accessible only from type and inherited types
        /// </summary>
        FamilyType = 0x08,

        /// <summary>
        /// Type is private to assembly
        /// Yes, that's "assembly"
        /// </summary>
        PrivateType = 0x10,
        /// <summary>
        /// Private member
        /// </summary>
        PrivateMember = 0x20,
        /// <summary>
        /// Anything private (that won't go out of the assembly)
        /// </summary>
        Private = PrivateType | PrivateMember,

        /// <summary>
        /// The assembly member
        /// (A member accessible from anywhere in assembly)
        /// </summary>
        AssemblyMember = 0x40,

        /// <summary>
        /// The family or assembly member
        /// </summary>
        FamilyOrAssemblyMember = 0x100,
        /// <summary>
        /// The family or assembly type
        /// </summary>
        FamilyOrAssemblyType = 0x200,

        /// <summary>
        /// The family and assembly member
        /// </summary>
        FamilyAndAssemblyMember = 0x1000,
        /// <summary>
        /// The family and assembly type
        /// </summary>
        FamilyAndAssemblyType = 0x1000,

        /// <summary>
        /// Matches any type visibility
        /// </summary>
        AnyType = PublicType | PrivateType | FamilyType | FamilyAndAssemblyType | FamilyOrAssemblyType,
        /// <summary>
        /// Matches any member visibility
        /// </summary>
        AnyMember = PublicMember | FamilyMember | PrivateMember | AssemblyMember | FamilyOrAssemblyMember | FamilyAndAssemblyMember,

        /// <summary>
        /// Any visiblity
        /// </summary>
        AnyVisiblity = AnyType | AnyMember,

        /// <summary>
        /// Protected (for C# familiars)
        /// </summary>
        ProtectedMember = FamilyMember,

        /// <summary>
        /// Internal (for C# familiars)
        /// </summary>
        InternalMember = AssemblyMember,

        /// <summary>
        /// Internal type (for C# dudes)
        /// </summary>
        InternalType = PrivateType,

        /// <summary>
        /// Protected internal (for C# friends)
        /// </summary>
        ProtectedInternalMember = FamilyOrAssemblyMember,
    }

    /// <summary>
    /// Exclusion filters for pointcuts.
    /// This has to be applied on advices
    /// </summary>
    /// <seealso cref="PointcutAttribute" />
    public sealed class ExcludePointcutAttribute : PointcutAttribute
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="PointcutAttribute" /> is include.
        /// </summary>
        /// <value>
        ///   <c>true</c> if include; otherwise, <c>false</c>.
        /// </value>
        public override bool Include => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludePointcutAttribute"/> class.
        /// </summary>
        public ExcludePointcutAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludePointcutAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public ExcludePointcutAttribute(params string[] names)
            : base(names)
        { }
    }

    /// <summary>
    /// Exclusion filters for pointcuts.
    /// This has to be applied on advices
    /// </summary>
    /// <seealso cref="PointcutAttribute" />
    public sealed class IncludePointcutAttribute : PointcutAttribute
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="PointcutAttribute" /> is include.
        /// </summary>
        /// <value>
        ///   <c>true</c> if include; otherwise, <c>false</c>.
        /// </value>
        public override bool Include => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludePointcutAttribute"/> class.
        /// </summary>
        public IncludePointcutAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludePointcutAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public IncludePointcutAttribute(params string[] names)
            : base(names)
        { }
    }

    /// <summary>
    /// Allows a class to be advice-proof, by specifying which advices won't apply
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ExcludeAdvicesAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name matching patterns.
        /// Default is extended Wildcard, Regex mode is enabled by using ^ at start or $ at end
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string[] Names { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAdvicesAttribute"/> class.
        /// </summary>
        public ExcludeAdvicesAttribute()
            : this(new string[0])
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAdvicesAttribute"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public ExcludeAdvicesAttribute(params string[] names)
        {
            Names = names;
        }
    }
}
