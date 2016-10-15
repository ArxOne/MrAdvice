#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using Advice;
    using dnlib.DotNet;
    using IO;
    using Reflection;
    using StitcherBoy.Weaving.Build;
    using Weaver;

    public class MrAdviceStitcher : AssemblyStitcher
    {
        public MrAdviceStitcher()
        {
            Logger.LogInfo = s => Logging.Write(s);
            Logger.LogWarning = s => Logging.WriteWarning(s);
            Logger.LogError = s => Logging.WriteError(s);
        }

        protected override bool Process(AssemblyStitcherContext context)
        {
            // instances are created here
            // please also note poor man's dependency injection (which is enough for us here)
            //var assemblyResolver = new DependenciesAssemblyResolver(context.Project.References.Select(r => new Dependency(r)));
            var assemblyResolver = context.AssemblyResolver;
            var typeResolver = new TypeResolver { AssemblyResolver = assemblyResolver };
            var typeLoader = new TypeLoader(() => LoadWeavedAssembly(context, assemblyResolver));
            var aspectWeaver = new AspectWeaver { TypeResolver = typeResolver, TypeLoader = typeLoader };
            // TODO: use blobber's resolution (WTF?)
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            //AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => MrAdviceTask.AssemblyResolve(GetType().Assembly, e);
            aspectWeaver.Weave(context.Module);
            return true;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            // because versions may differ, we'll pretend they're all the same
            if (assemblyName.Name == "MrAdvice")
                return typeof(IAdvice).Assembly;

            var assemblyData = ResolveAssembly(assemblyName);
            if (assemblyData == null)
                return null;

            return Assembly.Load(assemblyData);
        }

        /// <summary>
        /// Resolves the assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public static byte[] ResolveAssembly(AssemblyName assemblyName)
        {
            var resourceName = $"blobber:embedded.gz:{assemblyName.Name}";

            using (var resourceStream = typeof(MrAdviceStitcher).Assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;
                using (var gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress))
                using (var memoryStream = new MemoryStream())
                {
                    gzipStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Loads the weaved assembly.
        /// </summary>
        /// <returns></returns>
        private static Assembly LoadWeavedAssembly(AssemblyStitcherContext context, IAssemblyResolver assemblyResolver)
        {
            foreach (var assemblyRef in context.Module.GetAssemblyRefs())
            {
                try
                {
                    var assemblyRefName = new AssemblyName(assemblyRef.FullName);
                    if (assemblyRefName.Name == "MrAdvice")
                        continue;
                    var referencePath = assemblyResolver.Resolve(assemblyRef, context.Module).ManifestModule.Location;
                    var fileName = Path.GetFileName(referencePath);
                    // right, this is dirty!
                    if (fileName == "MrAdvice.dll" && AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "MrAdvice"))
                        continue;

                    //if (string.IsNullOrEmpty(referencePath))
                    //{
                    //    Logger.WriteDebug("Loading assembly from {0}", referencePath);
                    //}

                    var referenceBytes = File.ReadAllBytes(referencePath);
                    Assembly.Load(referenceBytes);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("Can't load {0}: {1}", assemblyRef.FullName, e.ToString());
                }
            }
            var bytes = File.ReadAllBytes(context.Module.Assembly.ManifestModule.Location);
            return Assembly.Load(bytes);
        }
    }
}
