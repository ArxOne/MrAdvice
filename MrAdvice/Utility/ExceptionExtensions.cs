#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

using System.Runtime.ExceptionServices;

namespace ArxOne.MrAdvice.Utility
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Extensions to exception
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Preserves the stack trace.
        /// I know there is a better solution in .NET 4.5, but this is PCL here
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static Exception Rethrow(this Exception exception)
        {
#if NETCOREAPP
            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;
#else
            var preserveStackTrace = typeof(Exception).GetMembersReader().GetMethod("PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic)
                                     ?? typeof(Exception).GetMembersReader().GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                preserveStackTrace?.Invoke(exception, Array.Empty<object>());
            }
            catch (MemberAccessException) { }
            throw exception;
#endif
        }
    }
}
