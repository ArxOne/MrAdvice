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
    using Utility;

    internal class WeaverMethodWeavingContext : MethodWeavingContext
    {
        private readonly TypeDefinition _typeDefinition;
        private readonly Types _types;

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
        /// <param name="targetMethodName">Name of the target method.</param>
        /// <param name="types">The types.</param>
        public WeaverMethodWeavingContext(TypeDefinition typeDefinition, Type type, string targetMethodName, Types types)
            : base(type, targetMethodName)
        {
            _typeDefinition = typeDefinition;
            _types = types;
        }

        /// <summary>
        /// Adds the public automatic property.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        public override void AddPublicAutoProperty(string propertyName, Type propertyType)
        {
            var moduleDefinition = _typeDefinition.Module;
            _typeDefinition.AddPublicAutoProperty(propertyName, moduleDefinition.Import(propertyType));
        }
    }
}
