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
    /// Allows to continue tasks, using reflection instead of generics
    /// (fuck the generics when they are the terminal point)
    /// </summary>
    public abstract class TaskAccessor
    {
        /// <summary>
        /// Creates a <see cref="TaskAccessor" /> for given result type.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns></returns>
        public static TaskAccessor Create(Task task)
        {
            var taskType = task.GetTaskType();
            if (taskType == typeof(void) || taskType == null)
                throw new ArgumentException("Task must be a Task<>");
            var taType = typeof(TaskAccessor<>).MakeGenericType(taskType);
            var source = (TaskAccessor)Activator.CreateInstance(taType, task);
            return source;
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public abstract object Result { get; }
    }

    /// <summary>
    /// <see cref="TaskContinuer"/> implementation and specialization
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class TaskAccessor<TResult> : TaskAccessor
    {
        private readonly Task<TResult> _task;

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public override object Result => _task.Result;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAccessor{TResult}"/> class.
        /// </summary>
        /// <param name="task">The task.</param>
        public TaskAccessor(Task<TResult> task)
        {
            _task = task;
        }
    }
}
