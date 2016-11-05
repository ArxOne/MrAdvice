#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using System.Linq;
    using Annotation;
    using dnlib.DotNet;

    /// <summary>
    /// Reflection group, type level
    /// </summary>
    internal class TypeReflectionNode : ReflectionNode
    {
        private readonly TypeDef _typeDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => new ModuleReflectionNode(_typeDefinition.Module, null);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren()
        {
            foreach (var propertyDefinition in _typeDefinition.Properties.OrderBy(p => p.Name))
                yield return new PropertyReflectionNode(propertyDefinition, this);
            foreach (var constructorMethodDefinition in _typeDefinition.FindConstructors())
                yield return new MethodReflectionNode(constructorMethodDefinition, this);
            foreach (var methodDefinition in _typeDefinition.Methods.OrderBy(m => m.Name).Where(m => !m.IsSpecialName))
                yield return new MethodReflectionNode(methodDefinition, this);
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => _typeDefinition.CustomAttributes;

        /// <summary>
        /// Gets a value indicating whether this instance is generic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is generic; otherwise, <c>false</c>.
        /// </value>
        public override bool IsGeneric => _typeDefinition.HasGenericParameters;

        public override MemberAttributes? Attributes
        {
            get
            {
                // For convenience, we ignore the nested idea... The day someone needs it --> https://github.com/ArxOne/MrAdvice/issues/new
                switch (_typeDefinition.Attributes & TypeAttributes.VisibilityMask)
                {
                    case TypeAttributes.Public:// = 1,
                    case TypeAttributes.NestedPublic:// = 2,
                        return MemberAttributes.PublicType;
                    case TypeAttributes.NestedPrivate:// = NestedPublic | Public = 3,
                        return MemberAttributes.PrivateType;
                    case TypeAttributes.NestedFamily:// = 4,
                        return MemberAttributes.FamilyType;
                    case TypeAttributes.NestedAssembly:// = NestedFamily | Public = 5,
                        return MemberAttributes.PrivateType;
                    case TypeAttributes.NestedFamANDAssem:// = NestedFamily | NestedPublic = 6,
                        return MemberAttributes.FamilyAndAssemblyType;
                    case TypeAttributes.NestedFamORAssem: // = NestedFamANDAssem | Public = 7,
                        return MemberAttributes.FamilyOrAssemblyType;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => _typeDefinition.FullName;

        private string DebugString => $"Type {_typeDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return DebugString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReflectionNode" /> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        /// <param name="parent">The parent.</param>
        public TypeReflectionNode(TypeDef typeDefinition, ReflectionNode parent)
        {
            _typeDefinition = typeDefinition;
            Parent = parent;
        }
    }
}