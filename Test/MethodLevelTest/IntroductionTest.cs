#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using Advices;
    using NUnit.Framework;

    [TestFixture]
    [Category("Introduction")]
    public class IntroductionTest
    {
        [Test]
        public void SimpleIntroductionByFieldTest()
        {
            var c = new IntroducedClass();
            c.AMethod();
            c.AMethod();

            Assert.That(IntroductionAdvice.LastAdvicesCount, Is.EqualTo(2));
        }

        [Test]
        public void SimpleStaticIntroductionByFieldTest()
        {
            var z = StaticIntroductionAdvice.LastStaticAdvicesCount;

            var c1 = new IntroducedClass();
            var c2 = new IntroducedClass();
            c1.BMethod();
            c2.BMethod();

            Assert.That(StaticIntroductionAdvice.LastStaticAdvicesCount - z, Is.EqualTo(2));
        }

        [Test]
        public void SimpleSharedIntroductionByFieldTest()
        {
            var z = SharedIntroductionAdvice.LastSharedAdvicesCount;

            var c1 = new IntroducedClass();
            c1.CMethod();
            c1.C2Method();

            Assert.That(SharedIntroductionAdvice.LastSharedAdvicesCount - z, Is.EqualTo(2));
        }

        [Test]
        public void SimpleIntroductionByPropertyTest()
        {
            var c = new IntroducedClass();
            c.AMethod();
            c.AMethod();

            Assert.That(IntroductionAdvice.LastRandomString, Is.EqualTo("10"));
        }

        [Test]
        public void ComplexIntroductionTest()
        {
            var c = new ComplexIntroducedClass();
            c.CMethod();
        }
    }
}