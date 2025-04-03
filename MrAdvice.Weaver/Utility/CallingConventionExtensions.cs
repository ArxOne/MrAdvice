#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace ArxOne.MrAdvice.Utility;

using System.Reflection;
using dnlib.DotNet;

public static class CallingConventionExtensions
{
    public static CallingConvention ToCallingConvention(this CallingConventions callingConventions)
    {
        CallingConvention callingConvention = 0;
        if (callingConventions.HasFlag(CallingConventions.HasThis))
            callingConvention |= CallingConvention.HasThis;
        if (callingConventions.HasFlag(CallingConventions.ExplicitThis))
            callingConvention |= CallingConvention.ExplicitThis; // not sure
        if (callingConventions.HasFlag(CallingConventions.Any))
            callingConvention |= CallingConvention.VarArg;
        //if (callingConventions.HasFlag(CallingConventions.Standard))
        //    callingConvention |= CallingConvention.StdCall;
        if (callingConventions.HasFlag(CallingConventions.VarArgs))
            callingConvention |= CallingConvention.VarArg;
        return callingConvention;
    }
}