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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using IO;

    public class DependenciesAssemblyResolver : IAssemblyResolver
    {
        private readonly IEnumerable<Dependency> _extraDependencies;
        private readonly IAssemblyResolver _assemblyResolver;

        public DependenciesAssemblyResolver(IEnumerable<Dependency> extraDependencies)
        {
            _extraDependencies = extraDependencies.ToArray();
            _assemblyResolver = new AssemblyResolver();
            foreach (var extraDependency in _extraDependencies)
                Logger.WriteDebug("Extra dependency: {0}", extraDependency.Path);
        }

        public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
        {
            return _assemblyResolver.Resolve(assembly, sourceModule) ?? ResolveDependency(assembly) ?? LoadEmbedded(assembly) /*?? DiagnoseNotFound(assembly, sourceModule)*/;
        }

        public bool AddToCache(AssemblyDef asm) => _assemblyResolver.AddToCache(asm);

        public bool Remove(AssemblyDef asm) => _assemblyResolver.Remove(asm);

        public void Clear() => _assemblyResolver.Clear();

        private AssemblyDef DiagnoseNotFound(IFullName assembly, ModuleDef sourceModule)
        {
            Logger.WriteError("Assembly {0} not found", assembly.FullName);
            foreach (var extraDependency in _extraDependencies)
                Logger.WriteWarning("**** Info: {0}", extraDependency.Info);

            // this is totally dirty and won't live more than a few days until I figure out the problem with Appveyor
            var sourceDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(sourceModule.Location))));
            var baseName = new AssemblyName(assembly.FullName).Name;
            Search(sourceDirectory, baseName + ".dll");
            Search(sourceDirectory, baseName + ".exe");

            return null;
        }

        private static void Search(string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
                Logger.WriteError("Found {0}", path);

            foreach (var subDirectory in Directory.GetDirectories(directory))
                Search(subDirectory, fileName);
        }

        private AssemblyDef ResolveDependency(IFullName assembly)
        {
            var assemblyName = new AssemblyName(assembly.FullName);
            Logger.WriteDebug("FindDependencies: {0}", assembly.FullName);
            foreach (var dependencyPath in _extraDependencies.Select(d => d.Path))
            {
                if (string.IsNullOrEmpty(dependencyPath) || !File.Exists(dependencyPath))
                    continue;
                var fileName = Path.GetFileNameWithoutExtension(dependencyPath);
                if (string.Equals(fileName, assemblyName.Name))
                {
                    try
                    {
                        var assemblyBytes = File.ReadAllBytes(dependencyPath);
                        var assemblyDef = AssemblyDef.Load(assemblyBytes);
                        assemblyDef.ManifestModule.Location = dependencyPath;
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

        /// <summary>
        /// Loads embedded assembly. This is used only to find MrAdvice, which from some unknown reasons can not be loaded otherwise
        /// </summary>
        /// <param name="assemblyNameReference">The assembly name reference.</param>
        /// <returns></returns>
        private static AssemblyDef LoadEmbedded(IFullName assemblyNameReference)
        {
            var assemblyName = new AssemblyName(assemblyNameReference.FullName);
            var assemblyData = MrAdviceStitcher.ResolveAssembly(assemblyName);
            if (assemblyData == null)
                return null;
            return AssemblyDef.Load(assemblyData);
        }
    }
}
