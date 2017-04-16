#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MrAdvice.Advice
{
    using System.Reflection;

    /// <summary>
    /// This delegate is used internally to speedup advices
    /// Invoking a delegate is much faster than invoking a <see cref="MethodInfo"/>
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public delegate object ProceedDelegate(object instance, object[] parameters);
}
