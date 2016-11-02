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

    internal class TaskCompletionSourceImplementation<TResult> : TaskCompletionSource
    {
        private readonly System.Threading.Tasks.TaskCompletionSource<TResult> _source;

        public override Task Task => _source.Task;

        /// <summary>
        ///     Sets the result.
        /// </summary>
        /// <param name="result">The result.</param>
        public override void SetResult(object result)
        {
            _source.SetResult((TResult)result);
        }

        /// <summary>
        ///     Sets the exception.
        /// </summary>
        /// <param name="e">The e.</param>
        public override void SetException(Exception e)
        {
            _source.SetException(e);
        }

        /// <summary>
        /// Sets to canceled state.
        /// </summary>
        public override void SetCanceled()
        {
            _source.SetCanceled();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TaskCompletionSource" /> class.
        /// </summary>
        public TaskCompletionSourceImplementation()
        {
            _source = new TaskCompletionSource<TResult>();
        }

        /// <summary>
        /// Performs an action after current task is complete.
        /// Action is run asynchronously
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override Task ContinueWith(Action<Task> action)
        {
            return _source.Task.ContinueWith(delegate (Task<TResult> t)
            {
                action(t);
                return t.Result;
            });
        }
    }
}