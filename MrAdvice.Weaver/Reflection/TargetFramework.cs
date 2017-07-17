#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Text.RegularExpressions;

    public partial class TargetFramework
    {
        /// <summary>
        /// Gets the supported .NET framework version.
        /// </summary>
        /// <value>
        /// The net.
        /// </value>
        public Version Net { get; private set; }
        /// <summary>
        /// Gets the supported Silverlight version.
        /// </summary>
        /// <value>
        /// The silverlight.
        /// </value>
        public Version Silverlight { get; private set; }
        /// <summary>
        /// Gets the supported Windows Phone version.
        /// </summary>
        /// <value>
        /// The windows phone.
        /// </value>
        public Version WindowsPhone { get; private set; }


        /// <summary>
        /// Gets a value indicating whether this instance is a Portable Class Library.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is PCL; otherwise, <c>false</c>.
        /// </value>
        public bool IsPCL { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetFramework"/> class.
        /// </summary>
        /// <param name="literalFrameworkName">Name of the framework.</param>
        public TargetFramework(string literalFrameworkName)
        {
            // good explanations about identifiers here: http://blog.stephencleary.com/2012/05/framework-profiles-in-net.html
            // and details about profiles here http://embed.plnkr.co/03ck2dCtnJogBKHJ9EjY/preview
            var frameworkName = new FrameworkName(literalFrameworkName);
            switch (frameworkName.Identifier)
            {
                case ".NETFramework":
                    Net = frameworkName.Version;
                    break;
                case "Silverlight":
                    Silverlight = frameworkName.Version;
                    break;
                case "WindowsPhone":
                    WindowsPhone = frameworkName.Version;
                    break;
                case ".NETPortable":
                    IsPCL = true;
                    InitializePortable(frameworkName.Profile);
                    break;
            }
        }

        private static readonly Regex ProfileEx = new Regex(@"(?<profile>Profile\d*)[^\(]*\((?<details>[^\)]*)\)");

        /// <summary>
        /// Initializes from a portable profile.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <exception cref="System.ArgumentException"></exception>
        private void InitializePortable(string profile)
        {
            // profile comes in the form of
            // Profile1	.NET Portable Subset (.NET Framework 4, Silverlight 4, Windows 8, Windows Phone Silverlight 7, Xbox 360)
            // we have all know values in the PortableLiterals constant
            var match = ProfileEx.Matches(PortableLiterals).OfType<Match>().SingleOrDefault(m => m.Groups["profile"].Value == profile);
            if (match == null)
                return;
            var details = match.Groups["details"].Value;
            var values = details.Split(',').Select(s => s.Trim()).ToArray();
            foreach (var value in values)
            {
                if (CheckVersion(value, ".NET Framework", v => Net = v)
                    || CheckVersion(value, "Silverlight", v => Silverlight = v)
                    || CheckVersion(value, "Windows Phone Silverlight", v => { })
                    || CheckVersion(value, "Windows Phone", v => WindowsPhone = v)
                    || CheckVersion(value, "Windows", v => { }))
                    continue;
                throw new ArgumentException(string.Format("Unrecognized profile '{0}' (as '{1}'", profile, details));
            }
        }

        /// <summary>
        /// Checks if given value matches the given framework, and if yes, calls custom action and returns true.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="frameworkName">Name of the framework.</param>
        /// <param name="setVersion">The set version.</param>
        /// <returns></returns>
        private static bool CheckVersion(string value, string frameworkName, Action<Version> setVersion)
        {
            if (!value.StartsWith(frameworkName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            var literalVersion = value.Substring(frameworkName.Length).Trim();
            Version version;
            if (literalVersion.Contains("."))
                version = new Version(literalVersion);
            else
                version = new Version(int.Parse(literalVersion), 0);
            setVersion(version);
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetFramework"/> class.
        /// </summary>
        /// <param name="net">The net.</param>
        public TargetFramework(Version net)
        {
            Net = net;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var literals = new List<string>();
            if (Net != null)
                literals.Add(".NET " + GetLiteralVersion(Net, 2));
            if (Silverlight != null)
                literals.Add("Silverlight " + GetLiteralVersion(Silverlight));
            if (WindowsPhone != null)
                literals.Add("Windows Phone " + GetLiteralVersion(WindowsPhone));
            return (IsPCL ? "(PCL) " : "") + string.Join(", ", literals.ToArray());
        }

        /// <summary>
        /// Gets the literal version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="minimumNumbers">The minimum numbers.</param>
        /// <returns></returns>
        private static string GetLiteralVersion(Version version, int minimumNumbers = 1)
        {
            var literalBuilder = new StringBuilder();
            var values = new[] { version.Major, version.Minor, version.Build, version.Revision };
            for (int index = 0; index < values.Length;)
            {
                if (literalBuilder.Length > 0)
                    literalBuilder.Append('.');
                literalBuilder.Append(values[index]);
                if (++index < minimumNumbers)
                    continue;
                if (values.Skip(index).All(v => v <= 0))
                    break;
            }
            return literalBuilder.ToString();
        }
    }
}
