#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Weaver
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using IO;
    using Mono.Cecil;
    using Utility;

    public class WorkerAppDomainProvider
    {
        /// <summary>
        /// Gets or sets the assembly resolver.
        /// </summary>
        /// <value>
        /// The assembly resolver.
        /// </value>
        public IAssemblyResolver AssemblyResolver { get; set; }

        /// <summary>
        /// Gets or sets the weaver path.
        /// </summary>
        /// <value>
        /// The weaver path.
        /// </value>
        public string WeaverPath { get; set; }

        public WorkerAppDomain<TEntry> Load<TEntry>(AssemblyDefinition assemblyDefinition)
            where TEntry : MarshalByRefObject
        {
            var directory = Path.Combine(Path.GetTempPath(), "MrAdvice-" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(directory);

            // saves MrAdvice itself
            var entryAssemblyFileName = Write(WeaverPath, directory);

            // then save given assembly and dependencies
            var assemblyFileName = Write(assemblyDefinition, directory);

            foreach (var referencedModule in assemblyDefinition.MainModule.GetSelfAndReferences(AssemblyResolver, true, 100).Skip(1))
                Write(referencedModule.Assembly, directory);

            return new WorkerAppDomain<TEntry>(directory, entryAssemblyFileName, assemblyFileName);
        }

        private string Write(AssemblyDefinition assemblyDefinition, string directory)
        {
            var assemblyFileName = assemblyDefinition.Name.Name + ".dll";
            var path = Path.Combine(directory, assemblyFileName);
            assemblyDefinition.Write(path);
            return assemblyFileName;
        }

        private string Write(string assemblyPath, string directory)
        {
            var assemblyBytes = File.ReadAllBytes(WeaverPath);
            var assemblyFileName = Path.GetFileName(assemblyPath);
            var targetPath = Path.Combine(directory, assemblyFileName);
            File.WriteAllBytes(targetPath, assemblyBytes);
            return assemblyFileName;
        }
    }
}
