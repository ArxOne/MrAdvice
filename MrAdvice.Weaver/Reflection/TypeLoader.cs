#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using dnlib.DotNet;

    public class TypeLoader
    {
        private Action _assembliesLoader;

        private readonly IDictionary<string, Type> _typesByName = new Dictionary<string, Type>();

        public TypeLoader(Action assembliesLoader)
        {
            _assembliesLoader = assembliesLoader;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns></returns>
        public Type GetType(ITypeDefOrRef typeReference)
        {
            if (_assembliesLoader != null)
            {
                _assembliesLoader();
                _assembliesLoader = null;
            }
            lock (_typesByName)
            {
                var fullName = typeReference.FullName.Replace('/', '+');
                Type type;
                if (_typesByName.TryGetValue(fullName, out type))
                    return type;
                type = FindType(fullName);
                _typesByName[fullName] = type;
                return type;
            }
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(fullName)).FirstOrDefault(type => type != null);
        }
    }
}
