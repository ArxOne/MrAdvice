#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Collections.Generic;

namespace ArxOne.MrAdvice
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using IO;
    using Reflection;
    using StitcherBoy;
    using StitcherBoy.Logging;
    using StitcherBoy.Weaving.Build;
    using Weaver;

    public class MrAdviceStitcher : AssemblyStitcher
    {
        public override string Name => "MrAdvice";

        private ILogging _logging;

        /// <summary>
        /// Processes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        protected override bool Process(AssemblyStitcherContext context)
        {
            BlobberHelper.Setup();

#if DEBUG
            _logging = Logging;// new MultiLogging(new DefaultLogging(Logging), new FileLogging("MrAdvice.log"));
            _logging.WriteDebug("Start");
#else
            _logging = Logging;
#endif
            if (context.Module is null)
            {
                _logging.WriteError("Target assembly {0} could not be loaded", context.AssemblyPath);
                return false;
            }

            try
            {
                // instances are created here
                // please also note poor man's dependency injection (which is enough for us here)
                var assemblyResolver = context.AssemblyResolver;
                var typeResolver = new TypeResolver(context.Module, context.Dependencies) { Logging = _logging, AssemblyResolver = assemblyResolver };
                var bytes = File.ReadAllBytes(context.Module.Assembly.ManifestModule.Location);
                var typeAssembly = Assembly.Load(bytes);
                var typeLoader = new TypeLoader(a => TryLoad(a, context, assemblyResolver), typeAssembly);
                var aspectWeaver = new AspectWeaver { Logging = _logging, TypeResolver = typeResolver, TypeLoader = typeLoader };

                // second chance: someone had the marker file missing
                if (AspectWeaver.FindShortcutType(context.Module) is not null)
                    return false;

                return aspectWeaver.Weave(context.Module);
            }
            catch (Exception e)
            {
                _logging.WriteError("Internal error: {0}", e.ToString());
                for (var ie = e.InnerException; ie is not null; ie = ie.InnerException)
                    _logging.WriteError("Inner exception: {0}", e.ToString());
            }

            return false;
        }

        private IDictionary<string, Assembly> LoadWeavedAssembly(AssemblyStitcherContext context, IAssemblyResolver assemblyResolver)
        {
            return DoLoadWeavedAssembly(context, assemblyResolver).ToDictionary(t => t.Item1, t => t.Item2);
        }

        /// <summary>
        /// Loads the weaved assembly.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tuple<string, Assembly>> DoLoadWeavedAssembly(AssemblyStitcherContext context, IAssemblyResolver assemblyResolver)
        {
            foreach (var assemblyRef in context.Module.GetAssemblyRefs())
            {
                Assembly loadedAssembly = null;
                try
                {
                    var assemblyRefName = new AssemblyName(assemblyRef.FullName);
                    if (assemblyRefName.Name == "MrAdvice")
                        continue;
                    var referencePath = GetReferencePath(context, assemblyResolver, assemblyRef);
                    var fileName = Path.GetFileName(referencePath);
                    // right, this is dirty!
                    if (fileName == "MrAdvice.dll" && AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "MrAdvice"))
                        continue;

                    var referenceBytes = File.ReadAllBytes(referencePath);
                    try
                    {
                        loadedAssembly = Assembly.Load(referenceBytes);
                    }
                    catch (BadImageFormatException)
                    {
                    }

                    try
                    {
                        if (loadedAssembly is null)
                            loadedAssembly = Assembly.ReflectionOnlyLoad(referenceBytes);
                    }
                    catch (PlatformNotSupportedException)
                    {
                    }
                }
                catch (Exception e)
                {
                    _logging.WriteWarning("Can't load {0}: {1}", assemblyRef.FullName, e.ToString());
                }

                if (loadedAssembly is not null)
                    yield return Tuple.Create(assemblyRef.FullName, loadedAssembly);
            }

            var bytes = File.ReadAllBytes(context.Module.Assembly.ManifestModule.Location);
            yield return Tuple.Create(context.Module.Assembly.FullName, Assembly.Load(bytes));
        }

        private Assembly TryLoad(string assemblyName, AssemblyStitcherContext context, IAssemblyResolver assemblyResolver)
        {
            try
            {
                var assemblyRef = new AssemblyRefUser(assemblyName);
                if (assemblyRef.Name == "MrAdvice")
                    return null;
                var referencePath = GetReferencePath(context, assemblyResolver, assemblyRef);
                var fileName = Path.GetFileName(referencePath);
                // right, this is dirty!
                if (fileName == "MrAdvice.dll" && AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "MrAdvice"))
                    return null;

                return TryLoad(referencePath);
            }
            catch (Exception e)
            {
                _logging.WriteWarning("Can't load {0}: {1}", assemblyName, e.ToString());
            }

            return null;
        }

        private static Assembly TryLoad(string referencePath)
        {
            var referenceBytes = File.ReadAllBytes(referencePath);
            try
            {
                return Assembly.Load(referenceBytes);
            }
            catch (BadImageFormatException)
            {
            }

            try
            {
                return Assembly.ReflectionOnlyLoad(referenceBytes);
            }
            catch (PlatformNotSupportedException)
            {
            }

            return null;
        }

        private static string GetReferencePath(AssemblyStitcherContext context, IAssemblyResolver assemblyResolver, AssemblyRef assemblyRef)
        {
            return GetReferencePathFromManifest(context, assemblyResolver, assemblyRef)
                   ?? GetReferencePathFromName(context, assemblyRef)
                   ?? GetReferencePathFromDependencies(context, assemblyRef);
        }


        private static string GetReferencePathFromDependencies(AssemblyStitcherContext context, AssemblyRef assemblyRef)
        {
            var assemblyName = new AssemblyName(assemblyRef.Name);
            var dependencies = (from assemblyDependency in context.Dependencies
                                let assemblyDependencyName = Path.GetFileNameWithoutExtension(assemblyDependency.Path)
                                where string.Equals(assemblyDependencyName, assemblyName.Name, StringComparison.InvariantCultureIgnoreCase)
                                select assemblyDependency.Path).ToArray();
            if (dependencies.Length == 1)
                return dependencies[0];
            return null;
        }

        private static string GetReferencePathFromManifest(AssemblyStitcherContext context, IAssemblyResolver assemblyResolver, AssemblyRef assemblyRef)
        {
            return assemblyResolver.Resolve(assemblyRef, context.Module)?.ManifestModule?.Location;
        }

        private static string GetReferencePathFromName(AssemblyStitcherContext context, AssemblyRef assemblyRef)
        {
            var referenceDirectory = Path.GetDirectoryName(context.AssemblyPath);
            var assemblyName = assemblyRef.Name.ToString();
            return GetExisting(assemblyName + ".dll") ?? GetExisting(assemblyName + ".exe")
                ?? GetExisting(Path.Combine(referenceDirectory, assemblyName + ".dll")) ?? GetExisting(Path.Combine(referenceDirectory, assemblyName + ".exe"));
        }

        private static string GetExisting(string path)
        {
            if (File.Exists(path))
                return path;
            return null;
        }
    }
}
