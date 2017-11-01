#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice.Threading
{
    using System.Threading.Tasks;

    /// <summary>
    /// Tasks helper library
    /// </summary>
    public static class Tasks
    {
        private static Task _void;

        /// <summary>
        /// Gets a default empty task.
        /// </summary>
        /// <returns>
        ///     The ok.
        /// </returns>
        public static Task Void()
        {
            if (_void == null)
            {
                var tcsVoid = new TaskCompletionSource<object>();
                tcsVoid.SetResult(null);
                _void = tcsVoid.Task;
            }
            return _void;
        }
    }
}
