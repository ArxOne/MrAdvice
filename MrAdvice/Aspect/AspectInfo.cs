#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice.Aspect
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Annotation;

    /// <summary>
    /// Aspect, with pointcut and advices applied to it
    /// </summary>
    internal class AspectInfo
    {
        /// <summary>
        /// Gets the advices applied to pointcut in this aspect.
        /// </summary>
        /// <value>
        /// The advices.
        /// </value>
        public IList<AdviceInfo> Advices { get; private set; }
        /// <summary>
        /// Gets the pointcut method.
        /// </summary>
        /// <value>
        /// The pointcut method.
        /// </value>
        public MethodInfo PointcutMethod { get; private set; }
        /// <summary>
        /// Gets the pointcut property, if any (if method is related to property).
        /// </summary>
        /// <value>
        /// The pointcut property.
        /// </value>
        public PropertyInfo PointcutProperty { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is pointcut property setter.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is pointcut property setter; otherwise, <c>false</c>.
        /// </value>
        public bool IsPointcutPropertySetter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectInfo"/> class.
        /// </summary>
        /// <param name="advices">The advices.</param>
        /// <param name="pointcutMethod">The pointcut method.</param>
        /// <param name="pointcutProperty">The pointcut property.</param>
        /// <param name="isPointcutPropertySetter">if set to <c>true</c> [is pointcut property setter].</param>
        public AspectInfo(IEnumerable<AdviceInfo> advices, MethodInfo pointcutMethod, PropertyInfo pointcutProperty, bool isPointcutPropertySetter)
            : this(advices, pointcutMethod)
        {
            PointcutProperty = pointcutProperty;
            IsPointcutPropertySetter = isPointcutPropertySetter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectInfo"/> class.
        /// </summary>
        /// <param name="advices">The advices.</param>
        /// <param name="pointcutMethod">The pointcut method.</param>
        public AspectInfo(IEnumerable<AdviceInfo> advices, MethodInfo pointcutMethod)
        {
            Advices = advices.OrderBy(a => Priority.GetLevel(a.Advice)).ToArray();
            PointcutMethod = pointcutMethod;
        }

        /// <summary>
        /// Adds the advice and returns a new AspectInfo.
        /// </summary>
        /// <param name="adviceInfo">The advice information.</param>
        /// <returns></returns>
        public AspectInfo AddAdvice(AdviceInfo adviceInfo)
        {
            return new AspectInfo(Advices.Concat(new[] { adviceInfo }), PointcutMethod, PointcutProperty, IsPointcutPropertySetter);
        }
    }
}