#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Annotation
{
    using System;
    /// <summary>
    /// Applied to generated inner methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ExecutionPointAttribute : Attribute
    {
    }
}
