#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using Advices;

    public class ComplexIntroducedClass
    {
        [ComplexIntroductionAdvice]
        public void CMethod()
        {
        }
    }
}