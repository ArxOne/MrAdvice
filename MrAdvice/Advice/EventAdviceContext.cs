#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Advice
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Property advice context, passed to property advisors
    /// </summary>
    [DebuggerDisplay("Event: {TargetEvent}, {DebuggerAddRemove}")]
    public class EventAdviceContext : SyncAdviceContext
    {
        /// <summary>
        /// Gets or sets the event delegate.
        /// </summary>
        /// <value>
        /// The delegate.
        /// </value>
        public Delegate Value
        {
            get
            {
                return (Delegate)AdviceValues.Arguments[0];
            }
            set
            {
                AdviceValues.Arguments[0] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this context is an adder (the event.add method).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is adder; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdder { get; }

        /// <summary>
        /// Gets a value indicating whether this context is a remover (the event.remove method).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is remover; otherwise, <c>false</c>.
        /// </value>
        public bool IsRemover => !IsAdder;

        private string DebuggerAddRemove => IsAdder ? "Add" : "Remove";

        /// <summary>
        /// Gets the target event.
        /// </summary>
        /// <value>
        /// The target event.
        /// </value>
        public EventInfo TargetEvent { get; }

        /// <summary>
        /// Gets the name of the target.
        /// </summary>
        /// <value>
        /// The name of the target.
        /// </value>
        public override string TargetName => TargetEvent.Name;

        private readonly IEventAdvice _eventAdvice;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAdviceContext" /> class.
        /// </summary>
        /// <param name="eventAdvice">The event advice.</param>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="isAdder">if set to <c>true</c> [is adder].</param>
        /// <param name="adviceValues">The advice values.</param>
        /// <param name="nextAdviceContext">The next advice context.</param>
        internal EventAdviceContext(IEventAdvice eventAdvice, EventInfo eventInfo, bool isAdder, AdviceValues adviceValues, AdviceContext nextAdviceContext)
            : base(adviceValues, nextAdviceContext)
        {
            _eventAdvice = eventAdvice;
            TargetEvent = eventInfo;
            IsAdder = isAdder;
        }

        /// <summary>
        /// Invokes the current aspect (related to this instance).
        /// </summary>
        internal override Task Invoke()
        {
            _eventAdvice.Advise(this);
            return null;
        }
    }
}
