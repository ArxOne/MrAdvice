#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ExternalAdviceTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExternalTest
    {
        [TestMethod]
        [TestCategory("Weaving")]
        public void ExternalAdviceTest()
        {
            new EmptyExternalAdvisedClass().MethodTest();
        }
    }
}
