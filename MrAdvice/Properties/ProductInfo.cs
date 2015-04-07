#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Reflection;
using System.Resources;

[assembly: AssemblyDescription("MrAdvice allows to weave aspects at build-time (just like PostSharp, but free as in free beer). "
    + "Write your own aspects in the form of attributes and apply them to target methods or properties.")]
[assembly: AssemblyCompany("Arx One")]
[assembly: AssemblyProduct("MrAdvice")]
[assembly: AssemblyCopyright("MIT license http://opensource.org/licenses/mit-license.php")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

[assembly: AssemblyVersion(Product.Version)]

// ReSharper disable once CheckNamespace
internal static class Product
{
    public const string Version = "0.9.2";
}

