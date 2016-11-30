
#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Reflection;
using System.Resources;
using ArxOne.MrAdvice.Properties;

[assembly: AssemblyDescription("MrAdvice allows to weave aspects at build-time (just like PostSharp, but free as in free beer). Write your own aspects in the form of attributes and apply them to target methods or properties. This version does not require any dependency (on the opposite of MrAdvice.Fody which requires Fody).")]
[assembly: AssemblyCompany("Arx One")]
[assembly: AssemblyProduct("Mr. Advice")]
[assembly: AssemblyCopyright("MIT license http://opensource.org/licenses/mit-license.php")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

[assembly: AssemblyVersion("2")]
[assembly: AssemblyFileVersion("2.4.0")] // -test2

namespace ArxOne.MrAdvice.Properties
{
    /// <summary>
    /// Informations about product
    /// </summary>
    internal static class Product
    {
        /// <summary>
        /// The version
        /// </summary>
        public const string Version = "2.4.0";
    }
}
