#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Reflection;

namespace ArxOne.MrAdvice.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using dnlib.DotNet;

    public delegate Assembly TypeLoaderTryResolve(string assemblyName);

    public class TypeLoader
    {
        private readonly TypeLoaderTryResolve _tryResolve;
        private readonly Assembly[] _assemblies;

        private readonly IDictionary<string, Type> _typesByName = new Dictionary<string, Type>();

        public TypeLoader(TypeLoaderTryResolve tryResolve, params Assembly[] assemblies)
        {
            _tryResolve = tryResolve;
            _assemblies = assemblies;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns></returns>
        public Type GetType(ITypeDefOrRef typeReference)
        {
            lock (_typesByName)
            {
                var fullName = typeReference.FullName.Replace('/', '+');
                if (_typesByName.TryGetValue(fullName, out var type))
                    return type;

                Assembly ResolveAssembly(object sender, ResolveEventArgs args) => _tryResolve(args.Name);

                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
                type = FindType(fullName, typeReference.DefinitionAssembly.FullNameToken);
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
                _typesByName[fullName] = type;
                return type;
            }
        }

        private Type FindType(string fullName, string assemblyFullName)
        {
            Type GetType(Assembly assembly)
            {
                if (assembly is null)
                    return null;
                return TryGetType(() => assembly.GetType(fullName))
                       ?? TryGetType(() => assembly.GetTypes().FirstOrDefault(t => t.FullName == fullName))
                       ;
            }

            var ownerAssembly = _tryResolve(assemblyFullName);
            return new[] { ownerAssembly }.Concat(_assemblies).Concat(AppDomain.CurrentDomain.GetAssemblies()).Select(GetType).FirstOrDefault(type => type is not null);
        }

        private static Type TryGetType(Func<Type> getType)
        {
            try
            {
                return getType();
            }
            catch { }

            return null;
        }
    }
}
