#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    using System;
    using System.Reflection;

    public static class ReflectionUtility
    {
        public static RuntimeMethodHandle GetPortableMethodHandle(this MethodBase methodBase)
        {
            return methodBase.MethodHandle;
        }
    }
}
