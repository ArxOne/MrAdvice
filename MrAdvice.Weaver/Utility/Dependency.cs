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
    using IO;
    using StitcherBoy.Project;

    public class Dependency
    {
        public AssemblyReference AssemblyReference { get; }

        public string Path => GetPath();

        public string Info => GetInfo();

        public Dependency(AssemblyReference assemblyReference)
        {
            AssemblyReference = assemblyReference;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns></returns>
        private string GetPath()
        {
            var projectDefinition = AssemblyReference.ProjectDefinition;
            if (projectDefinition == null)
                return AssemblyReference.Path;

            // the dependency may be found here:
            // - relative to its project
            // - in its relative outdir
            // - with the target file name
            var projectDir = System.IO.Path.GetDirectoryName(projectDefinition.ProjectPath);
            var outDir = projectDefinition.GetProperty("OutDir");
            if (string.IsNullOrEmpty(outDir))
                outDir = $"bin\\{projectDefinition.GetProperty("ConfigurationName")}";
            var targetFileName = projectDefinition.GetProperty("TargetFileName");
            Logger.WriteDebug("{0} {1} {2}", projectDir, outDir, targetFileName);
            return System.IO.Path.Combine(projectDir, outDir, targetFileName);
        }

        private string GetInfo()
        {
            var infoLines = new List<string> { GetPath() };
            var projectDefinition = AssemblyReference.ProjectDefinition;
            if (projectDefinition != null)
            {
                infoLines.Add($"---- Keys for {AssemblyReference.Name}");
                foreach (var k in projectDefinition.PropertiesKeys)
                {
                    try
                    {
                        infoLines.Add($"Key {k}: {projectDefinition.GetProperty(k)}");
                    }
                    catch
                    {
                        infoLines.Add($"Key {k}: ouch");
                    }
                }
            }
            return string.Join(Environment.NewLine, infoLines);
        }
    }
}
