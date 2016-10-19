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
    public abstract class TaskContinuer
    {
        /// <summary>
        /// Continues the specified task.
        /// </summary>
        /// <typeparam name="TTask">The type of the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public abstract Task ContinueWith<TTask>(TTask task, Func<TTask, object> func)
            where TTask : Task;


        /// <summary>
        /// Continues the specified two tasks and returns result of last task.
        /// </summary>
        /// <param name="waitTask">The wait task.</param>
        /// <param name="resultTask">The result task.</param>
        /// <returns></returns>
        public abstract Task ContinueWith(Task waitTask, Task resultTask);

        /// <summary>
        /// Creates a <see cref="TaskContinuer"/> for given result type.
        /// </summary>
        /// <param name="resultType">Type of the task.</param>
        /// <returns></returns>
        public static TaskContinuer Create(Type resultType)
        {
            var tcArgumentType = resultType == typeof(void) || resultType == null ? typeof(object) : resultType;
            var tcType = typeof(TaskContinuer<>).MakeGenericType(tcArgumentType);
            var source = (TaskContinuer)Activator.CreateInstance(tcType);
            return source;
        }

        /// <summary>
        /// Continues the task
        /// </summary>
        /// <typeparam name="TTask">The type of the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="func">The function.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        public static Task ContinueWith<TTask>(TTask task, Func<TTask, object> func, Type resultType)
            where TTask : Task
        {
            return Create(resultType).ContinueWith(task, func);
        }

        /// <summary>
        /// Continues the specified two tasks and returns result of last task.
        /// </summary>
        /// <param name="waitTask">The wait task.</param>
        /// <param name="resultTask">The result task.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns></returns>
        public static Task ContinueWith(Task waitTask, Task resultTask, Type resultType)
        {
            return Create(resultType).ContinueWith(waitTask, resultTask);
        }
    }

    /// <summary>
    /// <see cref="TaskContinuer"/> implementation and specialization
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class TaskContinuer<TResult> : TaskContinuer
    {
        /// <summary>
        /// Continues the specified task.
        /// </summary>
        /// <typeparam name="TTask">The type of the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public override Task ContinueWith<TTask>(TTask task, Func<TTask, object> func)
        {
            return task.ContinueWith(delegate (Task t)
            {
                var typedTask = (TTask)t;
                var result = func(typedTask);
                return (TResult)result;
            });
        }

        /// <summary>
        /// Continues the specified two tasks and returns result of last task.
        /// </summary>
        /// <param name="waitTask">The wait task.</param>
        /// <param name="resultTask">The result task.</param>
        /// <returns></returns>
        public override Task ContinueWith(Task waitTask, Task resultTask)
        {
            var typedResultTask = (Task<TResult>)resultTask;
            var continuationTask = Task<TResult>.Factory.ContinueWhenAll(new[] { waitTask, resultTask }, t => typedResultTask.Result);
            return continuationTask;
        }
    }
}
