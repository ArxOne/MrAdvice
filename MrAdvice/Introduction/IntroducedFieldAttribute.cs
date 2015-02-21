#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Introduction
{
    using System;

    /// <summary>
    /// Internal marker to match introduced fields and advices
    /// just in case some obfuscator would rename the fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IntroducedFieldAttribute : Attribute
    {
        /// <summary>
        /// The link identifier
        /// </summary>
        public string LinkID;
    }
}
