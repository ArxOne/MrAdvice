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
    using dnlib.DotNet;
    using IO;
    using Reflection;
    using StitcherBoy.Weaving;
    using Weaver;

    public class MrAdviceStitcher : SingleStitcher
    {
        protected override bool Process(StitcherContext context)
        {
            // instances are created here
            // please also note poor man's dependency injection (which is enough for us here)
            Logger.LogInfo = s => Logging.Write(s);
            Logger.LogWarning = s => Logging.WriteWarning(s);
            Logger.LogError = s => Logging.WriteError(s);
            var assemblyResolver = new AssemblyResolver();
            var typeResolver = new TypeResolver { AssemblyResolver = assemblyResolver };
            var typeLoader = new TypeLoader(() => LoadWeavedAssembly(context, assemblyResolver));
            var aspectWeaver = new AspectWeaver { TypeResolver = typeResolver, TypeLoader = typeLoader };
            // TODO: use blobber's resolution (WTF?)
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            //AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => MrAdviceTask.AssemblyResolve(GetType().Assembly, e);
            aspectWeaver.Weave(context.Module);
            return true;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var resourceName = $"blobber:embedded.gz:{assemblyName.Name}";

            using (var resourceStream = GetType().Assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    return null;
                using (var gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress))
                using (var memoryStream = new MemoryStream())
                {
                    gzipStream.CopyTo(memoryStream);
                    var assembly = Assembly.Load(memoryStream.ToArray());
                    return assembly;
                }
            }
        }

        /// <summary>
        /// Loads the weaved assembly.
        /// </summary>
        /// <returns></returns>
        private static Assembly LoadWeavedAssembly(StitcherContext context, IAssemblyResolver assemblyResolver)
        {
            foreach (var assemblyRef in context.Module.GetAssemblyRefs())
            {
                var referencePath = assemblyResolver.Resolve(assemblyRef, context.Module).ManifestModule.Location;
                try
                {
                    var fileName = Path.GetFileName(referencePath);
                    // right, this is dirty!
                    if (fileName == "MrAdvice.dll" && AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "MrAdvice"))
                        continue;
                    var referenceBytes = File.ReadAllBytes(referencePath);
                    Assembly.Load(referenceBytes);
                }
                catch (Exception e)
                {
                    Logger.WriteWarning("Can't load {0}: {1}", referencePath, e.GetType().Name);
                }
            }
            var bytes = File.ReadAllBytes(context.Module.Assembly.ManifestModule.Location);
            return Assembly.Load(bytes);
        }
    }
}
