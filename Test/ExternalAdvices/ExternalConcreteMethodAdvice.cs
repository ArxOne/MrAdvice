#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ExternalAdvices
{
    using ArxOne.MrAdvice.Advice;

    public class ExternalConcreteMethodAdvice : AbstractMethodAdvice
    {
        public override void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }
}