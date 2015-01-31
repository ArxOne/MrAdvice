#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.Weavisor.Advice
{
    /// <summary>
    /// Advice related call context (each advice received a different Call with a common CallContext)
    /// This is given to advices
    /// </summary>
    /// <typeparam name="TCallContext">The type of the call context.</typeparam>
    public class Call<TCallContext>
        where TCallContext: CallContext
    {
        private readonly int _step;
      
        /// <summary>
        /// Gets the call context.
        /// </summary>
        /// <value>
        /// The call context.
        /// </value>
        public TCallContext Context { get; private set; }

        internal Call(TCallContext context, int step)
        {
            _step = step;
            Context = context;
        }

        /// <summary>
        /// Proceeds to next advice or final target.
        /// </summary>
        public void Proceed()
        {
            Context.Proceed(_step + 1);
        }
    }
}
