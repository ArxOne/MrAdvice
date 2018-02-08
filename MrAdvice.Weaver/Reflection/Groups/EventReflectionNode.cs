#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection.Groups
{
    using System.Collections.Generic;
    using dnlib.DotNet;

    /// <summary>
    /// Reflection group, property level
    /// </summary>
    internal class EventReflectionNode : ReflectionNode
    {
        private readonly EventDef _eventDefinition;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>
        /// The parent, or null if top-level.
        /// </value>
        protected override ReflectionNode LoadParent() => new TypeReflectionNode(_eventDefinition.DeclaringType, null);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        protected override IEnumerable<ReflectionNode> LoadChildren()
        {
            if (_eventDefinition.AddMethod != null)
                yield return new MethodReflectionNode(_eventDefinition.AddMethod, this, _eventDefinition);
            if (_eventDefinition.RemoveMethod != null)
                yield return new MethodReflectionNode(_eventDefinition.RemoveMethod, this, _eventDefinition);
        }

        /// <summary>
        /// Gets the custom attributes at this level.
        /// </summary>
        /// <value>
        /// The custom attributes.
        /// </value>
        public override IEnumerable<CustomAttribute> CustomAttributes => _eventDefinition.CustomAttributes;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name => _eventDefinition.Name;

        private string DebugString => $"Event {_eventDefinition.FullName}";

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => DebugString;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyReflectionNode" /> class.
        /// </summary>
        /// <param name="eventDefinition">The event definition.</param>
        /// <param name="parent">The parent.</param>
        public EventReflectionNode(EventDef eventDefinition, TypeReflectionNode parent)
        {
            _eventDefinition = eventDefinition;
            Parent = parent;
        }
    }
}