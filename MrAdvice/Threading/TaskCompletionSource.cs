#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Threading
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Task completion source
    /// </summary>
    public abstract class TaskCompletionSource
    {
        /// <summary>
        /// Gets the task.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public abstract Task Task { get; }

        /// <summary>
        ///     Sets the result.
        /// </summary>
        /// <param name="result">The result.</param>
        public abstract void SetResult(object result);

        /// <summary>
        ///     Sets the exception.
        /// </summary>
        /// <param name="e">The e.</param>
        public abstract void SetException(Exception e);

        /// <summary>
        /// Sets to canceled state.
        /// </summary>
        public abstract void SetCanceled();

        /// <summary>
        /// Creates a TaskCompletionSource for the given.
        /// </summary>
        /// <param name="taskType">Type of the task
        /// (may be void or null, in which case the result parameter to SetResult() is ignored).</param>
        /// <returns></returns>
        public static TaskCompletionSource Create(Type taskType)
        {
            var tcsArgumentType = taskType == typeof(void) || taskType == null ? typeof(object) : taskType;
            var tcsType = typeof(TaskCompletionSourceImplementation<>).MakeGenericType(tcsArgumentType);
            var source = (TaskCompletionSource)Activator.CreateInstance(tcsType);
            return source;
        }

        /// <summary>
        /// Performs an action after current task is complete.
        /// Action is run asynchronously
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public abstract Task ContinueWith(Action<Task> action);
    }
}