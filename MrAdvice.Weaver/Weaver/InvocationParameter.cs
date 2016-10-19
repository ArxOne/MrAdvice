#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;

    public class InvocationParameter
    {
        /// <summary>
        /// Emits code to load value.
        /// </summary>
        /// <value>
        /// The emit value.
        /// </value>
        public Action<Instructions> EmitValue { get; }
        /// <summary>
        /// Emits code to load default value.
        /// </summary>
        /// <value>
        /// The emit default.
        /// </value>
        public Action<Instructions> EmitDefault { get; }

        public bool HasValue { get; }

        /// <summary>
        /// Emits the code to load value or default.
        /// </summary>
        /// <value>
        /// The emit.
        /// </value>
        public Action<Instructions> Emit => HasValue ? EmitValue : EmitDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvocationParameter"/> class.
        /// </summary>
        /// <param name="hasValue">if set to <c>true</c> [has value].</param>
        /// <param name="emitValue">The emit value.</param>
        /// <param name="emitDefault">The emit default.</param>
        public InvocationParameter(bool hasValue, Action<Instructions> emitValue, Action<Instructions> emitDefault)
        {
            EmitValue = emitValue;
            EmitDefault = emitDefault;
            HasValue = hasValue;
        }
    }
}
