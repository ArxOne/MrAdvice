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
    /// Pointcut member matching attributes
    /// </summary>
    [Flags]
    public enum VisibilityScope
    {
        /// <summary>
        /// Public type
        /// </summary>
        PublicGlobalType = 0x0001,
        /// <summary>
        /// Public nested type
        /// </summary>
        PublicNestedType = 0x0002,
        /// <summary>
        /// Public member
        /// </summary>
        PublicMember = 0x0008,
        /// <summary>
        /// Any public type
        /// </summary>
        PublicType = PublicGlobalType | PublicNestedType,

        /// <summary>
        /// Family nested type
        /// </summary>
        FamilyNestedType = 0x0020,
        /// <summary>
        /// Member is accessible only from type and inherited types
        /// Yes, that's "protected"
        /// </summary>
        FamilyMember = 0x0080,

        /// <summary>
        /// Family type
        /// </summary>
        FamilyType = FamilyNestedType,

        /// <summary>
        /// Type is private to assembly
        /// Yes, that's "assembly"
        /// </summary>
        PrivateGlobalType = 0x0100,
        /// <summary>
        /// Private nested type
        /// </summary>
        PrivateNestedType = 0x0200,
        /// <summary>
        /// Private member
        /// </summary>
        PrivateMember = 0x0800,
        /// <summary>
        /// Anything private (that won't go out of the assembly)
        /// </summary>
        PrivateType = PrivateGlobalType | PrivateNestedType,

        /// <summary>
        /// The assembly nested type
        /// A nested type accessible from everywhere in assembly
        /// </summary>
        AssemblyNestedType = 0x1000,

        /// <summary>
        /// The assembly member
        /// (A member accessible from anywhere in assembly)
        /// </summary>
        AssemblyMember = 0x2000,

        /// <summary>
        /// The family or assembly member
        /// </summary>
        FamilyOrAssemblyMember = 0x10000,
        /// <summary>
        /// The family or assembly type
        /// </summary>
        FamilyOrAssemblyType = 0x20000,

        /// <summary>
        /// The family and assembly member
        /// </summary>
        FamilyAndAssemblyMember = 0x100000,
        /// <summary>
        /// The family and assembly type
        /// </summary>
        FamilyAndAssemblyType = 0x200000,

        /// <summary>
        /// Matches any type visibility
        /// </summary>
        AnyType = PublicType | FamilyType | PrivateType | AssemblyNestedType | FamilyOrAssemblyType | FamilyAndAssemblyType,
        /// <summary>
        /// Matches any member visibility
        /// </summary>
        AnyMember = PublicMember | FamilyMember | PrivateMember | AssemblyMember | FamilyOrAssemblyMember | FamilyAndAssemblyMember,

        /// <summary>
        /// Any visiblity
        /// </summary>
        AnyAccessibility = AnyType | AnyMember,

        /// <summary>
        /// Any attribute. ANY. ANY!
        /// </summary>
        Any = AnyAccessibility,

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
        InternalType = PrivateGlobalType,

        /// <summary>
        /// Protected internal (for C# friends)
        /// </summary>
        ProtectedInternalMember = FamilyOrAssemblyMember,
    }
}