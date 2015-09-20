#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Advice;
    using Mono.Cecil;

    internal class WeaverMethodWeavingContext : MethodWeavingContext
    {
        private readonly TypeDefinition _typeDefinition;

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public virtual IEnumerable<string> Properties
        {
            get { return _typeDefinition.Properties.Select(p => p.Name); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeaverMethodWeavingContext" /> class.
        /// </summary>
        /// <param name="typeDefinition">The type definition (type being built).</param>
        /// <param name="type">The type (original type).</param>
        public WeaverMethodWeavingContext(TypeDefinition typeDefinition, Type type)
            : base(type)
        {
            _typeDefinition = typeDefinition;
        }

        public override bool AddPublicAutoProperty(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}
