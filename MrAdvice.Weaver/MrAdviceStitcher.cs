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
            // Prevent internal error during: System.IO.IOException: The process cannot access the file 'C:\git\MrAdvice\Test\TestApplication\MrAdvice.log' because it is being used by another process.
            _logging = new MultiLogging(new DefaultLogging(Logging), new FileLogging($"MrAdvice_{System.Diagnostics.Process.GetCurrentProcess().Id}.log"));
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
                var typeLoader = new TypeLoader(() => LoadWeavedAssembly(context, assemblyResolver));
                var aspectWeaver = new AspectWeaver { Logging = _logging, TypeResolver = typeResolver, TypeLoader = typeLoader };

                // second chance: someone had the marker file missing
                if (aspectWeaver.FindShortcutType(context.Module) is not null)
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

        /// <summary>
        /// Loads the weaved assembly.
        /// </summary>
        /// <returns></returns>
        private Assembly LoadWeavedAssembly(AssemblyStitcherContext context, IAssemblyResolver assemblyResolver)
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

                    var referenceBytes = File.ReadAllBytes(referencePath);
                    try
                    {
                        Assembly.Load(referenceBytes);
                    }
                    catch (BadImageFormatException) { }

                    try
                    {
                        Assembly.ReflectionOnlyLoad(referenceBytes);
                    }
                    catch (PlatformNotSupportedException) { }
                }
                catch (Exception e)
                {
                    _logging.WriteWarning("Can't load {0}: {1}", assemblyRef.FullName, e.ToString());
                }
            }
            var bytes = File.ReadAllBytes(context.Module.Assembly.ManifestModule.Location);
            return Assembly.Load(bytes);
        }
    }
}
