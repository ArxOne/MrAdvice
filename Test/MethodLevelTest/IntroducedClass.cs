#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using Advices;

    public class IntroducedClass
    {
        [IntroductionAdvice]
        public string AProperty { get; set; }

        [IntroductionAdvice]
        public void AMethod()
        { }

        [StaticIntroductionAdvice]
        public void BMethod()
        { }

        [SharedIntroductionAdvice]
        public void CMethod()
        { }

        [SharedIntroductionAdvice]
        public void C2Method()
        { }
    }
}
