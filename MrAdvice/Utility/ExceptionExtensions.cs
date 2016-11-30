#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

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
        public static Exception PreserveStackTrace(this Exception exception)
        {
            var preserveStackTrace = typeof(Exception).GetMembersReader().GetMethod("PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic)
                                     ?? typeof(Exception).GetMembersReader().GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                preserveStackTrace?.Invoke(exception, new object[0]);
            }
            catch (MemberAccessException) { }
            return exception;
        }
    }
}
