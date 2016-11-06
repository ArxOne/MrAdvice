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
}