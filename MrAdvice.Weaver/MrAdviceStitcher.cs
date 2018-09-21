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
        private ILogging _logging;

        protected override bool Process(AssemblyStitcherContext context)
        {
            BlobberHelper.Setup();

            if (AlreadyProcessed(context))
                return false;

#if DEBUG
            _logging = new MultiLogging(new DefaultLogging(Logging), new FileLogging("MrAdvice.log"));
            _logging.WriteDebug("Start");
#else
            _logging = Logging;
#endif
            if (context.Module == null)
            {
                _logging.WriteError("Target assembly {0} could not be loaded", context.AssemblyPath);
                return false;
            }

            try
            {
                try
                {
                    const string mrAdviceAssemblyName = "MrAdvice, Version=2.0.0.0, Culture=neutral, PublicKeyToken=c0e7e6eab6f293d8";
                    var mrAdviceAssembly = LoadEmbeddedAssembly(mrAdviceAssemblyName);
                    if (mrAdviceAssembly == null)
                    {
                        _logging.WriteError("Can't find/load embedded MrAdvice assembly (WTF?), exiting");
                        return false;
                    }
                }
                catch (FileNotFoundException)
                {
                    _logging.WriteError("Can't load MrAdvice assembly (WTF?), exiting");
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        _logging.Write("Assembly in AppDomain: {0}", assembly.GetName());
                    return false;
                }

                // instances are created here
                // please also note poor man's dependency injection (which is enough for us here)
                var assemblyResolver = context.AssemblyResolver;
                var typeResolver = new TypeResolver(context.Module) { Logging = _logging, AssemblyResolver = assemblyResolver };
                var typeLoader = new TypeLoader(() => LoadWeavedAssembly(context, assemblyResolver));
                var aspectWeaver = new AspectWeaver { Logging = _logging, TypeResolver = typeResolver, TypeLoader = typeLoader };

                // second chance: someone had the marker file missing
                if (aspectWeaver.FindShortcutType(context.Module) != null)
                    return false;

                return aspectWeaver.Weave(context.Module);
            }
            catch (Exception e)
            {
                _logging.WriteError("Internal error: {0}", e.ToString());
                for (var ie = e.InnerException; ie != null; ie = ie.InnerException)
                    _logging.WriteError("Inner exception: {0}", e.ToString());
            }
            return false;
        }

        private bool AlreadyProcessed(AssemblyStitcherContext context)
        {
            var processFilePath = GetProcessFilePath(context);
            var processed = GetLastWriteDate(processFilePath) >= GetLastWriteDate(context.AssemblyPath);
            if (!processed)
            {
                ModuleWritten += delegate
                {
                    File.WriteAllText(processFilePath,
                        "This file is a marker for Mr.Advice to ensure the assembly wasn't processed twice (in which case it would be as bad as crossing the streams).");
                };
            }
            return processed;
        }

        private static string GetProcessFilePath(AssemblyStitcherContext context)
        {
            return context.AssemblyPath + ".\u2665MrAdvice";
        }

        private static DateTime GetLastWriteDate(string path)
        {
            if (!File.Exists(path))
                return DateTime.MinValue;
            return new FileInfo(path).LastWriteTimeUtc;
        }

        private Assembly LoadEmbeddedAssembly(string assemblyName)
        {
            var existingAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName);
            if (existingAssembly != null)
                return existingAssembly;
            return BlobberHelper.LoadAssembly(GetType().Assembly, assemblyName);
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

                    //if (string.IsNullOrEmpty(referencePath))
                    //{
                    //    Logger.WriteDebug("Loading assembly from {0}", referencePath);
                    //}

                    var referenceBytes = File.ReadAllBytes(referencePath);
                    Assembly.Load(referenceBytes);
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
