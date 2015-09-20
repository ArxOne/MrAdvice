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
    using System.Reflection;
    using Mono.Cecil;

    public class AssemblyProvider
    {
        public Assembly Load(AssemblyDefinition assemblyDefinition)
        {
            try
            {
                var path = Write(assemblyDefinition, Path.GetTempPath());
                var assembly = Assembly.LoadFile(path);
                return assembly;
            }
            catch { }
            return null;

        }

        private string Write(AssemblyDefinition assemblyDefinition, string directory)
        {
            var path = Path.Combine(directory, assemblyDefinition.Name.Name + ".dll");
            assemblyDefinition.Write(path);
            return path;
        }
    }
}
