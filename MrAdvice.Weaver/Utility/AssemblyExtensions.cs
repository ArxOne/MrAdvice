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
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typeReference">The type reference.</param>
        /// <returns></returns>
        public static Type GetType(this Assembly assembly, TypeRef typeReference)
        {
            try
            {
                var fullName = typeReference.FullName.Replace('/', '+');

                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = a.GetType(fullName);
                    if (type != null)
                        return type;
                }

                //var type = assemblyTypes.FirstOrDefault(t => t.FullName == fullName)
                //    ?? assemblyTypes.FirstOrDefault(t => t.FullName == typeReference.FullName);

                //if (type == null)
                //{
                Logger.WriteError("Can't find type {0}", typeReference.FullName);
                //    Logger.Write("Available types:");
                //    foreach (var assemblyType in assemblyTypes)
                //        Logger.Write("- {0}", assemblyType.FullName);
                //}

                //return type;
                return null;
            }
            catch (ReflectionTypeLoadException e)
            {
                Logger.WriteError("Error while loading types from {0}: {1}\n{2}", assembly.FullName, e.ToString(),
                   string.Join(Environment.NewLine + "------------" + Environment.NewLine, e.LoaderExceptions.Select(le => le.ToString())));
                throw;
            }
        }

        public static ModuleDef GetMainModule(this AssemblyDef assemblyDef)
        {
            return assemblyDef.ManifestModule;
            //return assemblyDef.FindModule("<Module>");
        }
    }
}
