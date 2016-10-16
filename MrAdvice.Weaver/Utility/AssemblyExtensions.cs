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
    using System.Linq;
    using System.Reflection;
    using dnlib.DotNet;
    using IO;

    internal static class AssemblyExtensions
    {
        public static ModuleDef GetMainModule(this AssemblyDef assemblyDef)
        {
            return assemblyDef.ManifestModule;
            //return assemblyDef.FindModule("<Module>");
        }
    }
}
