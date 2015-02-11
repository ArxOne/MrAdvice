#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using Advices;

    public class IntroducedClass
    {
        [IntroductionAdvice]
        public void AMethod()
        { }

        [StaticIntroductionAdvice]
        public void BMethod()
        { }
    }
}
