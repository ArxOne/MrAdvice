#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System;

namespace MrAdvice.Annotation
{
    /// <summary>
    /// Defines a kind of advisable member
    /// </summary>
    [Flags]
    public enum MemberKind
    {
        /// <summary>
        /// The constructor
        /// </summary>
        Constructor = 0x0001,
        /// <summary>
        /// A method (that is not everything else)
        /// </summary>
        Method = 0x0002,
        /// <summary>
        /// A property getter
        /// </summary>
        PropertyGet = 0x0004,
        /// <summary>
        /// A property setter
        /// </summary>
        PropertySet = 0x0008,

        /// <summary>
        /// All property accessors
        /// </summary>
        PropertyGetSet = PropertyGet | PropertySet,

        /// <summary>
        /// An event adder
        /// </summary>
        EventAdd = 0x0010,
        /// <summary>
        /// An event remover
        /// </summary>
        EventRemove = 0x0010,

        /// <summary>
        /// All event manipulators
        /// </summary>
        EventAddRemove = EventAdd | EventRemove,

        /// <summary>
        /// Anything that is a <see cref="MethodInfo"/>
        /// </summary>
        MethodInfo = Method | PropertyGetSet | EventAddRemove,

        /// <summary>
        /// Anything
        /// </summary>
        Any = Constructor | MethodInfo,
    }
}
