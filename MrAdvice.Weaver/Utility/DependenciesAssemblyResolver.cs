#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using IO;

    public class DependenciesAssemblyResolver : IAssemblyResolver
    {
        private readonly IEnumerable<string> _extraDependencies;
        private readonly IAssemblyResolver _assemblyResolver;

        public DependenciesAssemblyResolver(IEnumerable<string> extraDependencies)
        {
            _extraDependencies = extraDependencies.ToArray();
            _assemblyResolver = new AssemblyResolver();
            foreach (var extraDependency in _extraDependencies)
                Logger.WriteDebug("Extra dependency: {0}", extraDependency);
        }

        public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
        {
            return _assemblyResolver.Resolve(assembly, sourceModule) ?? ResolveDependency(assembly);
        }

        public bool AddToCache(AssemblyDef asm) => _assemblyResolver.AddToCache(asm);

        public bool Remove(AssemblyDef asm) => _assemblyResolver.Remove(asm);

        public void Clear() => _assemblyResolver.Clear();

        private AssemblyDef ResolveDependency(IAssembly assembly)
        {
            var assemblyName = new AssemblyName(assembly.FullName);
            Logger.WriteDebug("FindDependencies: {0}", assembly.FullName);
            foreach (var extraDependency in _extraDependencies)
            {
                var fileName = Path.GetFileNameWithoutExtension(extraDependency);
                if (string.Equals(fileName, assemblyName.Name))
                {
                    try
                    {
                        var assemblyBytes = File.ReadAllBytes(extraDependency);
                        var assemblyDef = AssemblyDef.Load(assemblyBytes);
                        if (assemblyDef.FullName == assembly.FullName)
                            return assemblyDef;
                        Logger.WriteDebug("Expected '{0}', found '{1}'", assembly.FullName, assemblyDef.FullName);
                    }
                    catch (Exception e)
                    {
                        Logger.WriteError(e.ToString());
                    }
                }
            }
            return null;
        }
    }
}
