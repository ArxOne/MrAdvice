#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

using System;

namespace ArxOne.MrAdvice.Advice.Builder;

[Flags]
public enum WeaverAddFlags
{
    None = 0,
    /// <summary>
    /// When set, checks that the delegate was not already added (and ignores it if it was)
    /// </summary>
    Once = 0x0001,

    Default = Once,
}