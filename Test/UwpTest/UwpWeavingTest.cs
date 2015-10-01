#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace UwpTest
{
    using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

    [TestClass]
    public class UwpWeavingTest
    {
        public class AdvisedClass
        {
            [UwpAdvice]
            public int Add(int a, int b)
            {
                return a + b;
            }
        }

        [TestMethod]
        [TestCategory("UWP")]
        public void SimpleAdviceUwpTest()
        {
            var c = new AdvisedClass();
            var r = c.Add(3, 5);
            Assert.AreEqual(3 + 5 + 1, r);
        }
    }
}