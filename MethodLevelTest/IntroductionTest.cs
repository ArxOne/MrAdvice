#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System.Linq;
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
        public void SimpleIntroductionByPropertyTest()
        {
            var c = new IntroducedClass();
            c.AMethod();
            c.AMethod();

            Assert.AreEqual("10", IntroductionAdvice.LastRandomString);
        }
    }
}
