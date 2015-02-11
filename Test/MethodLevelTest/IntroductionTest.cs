#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IntroductionTest
    {
        [TestMethod]
        [TestCategory("Introduction")]
        public void SimpleIntroductionByFieldTest()
        {
            var c = new IntroducedClass();
            c.AMethod();
            c.AMethod();

            Assert.AreEqual(2, IntroductionAdvice.LastAdvicesCount);
        }

        [TestMethod]
        [TestCategory("Introduction")]
        public void SimpleStaticIntroductionByFieldTest()
        {
            var z = StaticIntroductionAdvice.LastStaticAdvicesCount;

            var c1 = new IntroducedClass();
            var c2 = new IntroducedClass();
            c1.BMethod();
            c2.BMethod();

            Assert.AreEqual(2, StaticIntroductionAdvice.LastStaticAdvicesCount - z);
        }

        [TestMethod]
        [TestCategory("Introduction")]
        public void SimpleIntroductionByPropertyTest()
        {
            var c = new IntroducedClass();
            c.AMethod();
            c.AMethod();

            Assert.AreEqual("10", IntroductionAdvice.LastRandomString);
        }
    }
}
