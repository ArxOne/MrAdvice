#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using NUnit.Framework;

namespace ExternalAdviceTest
{

    [TestFixture]
    [Category("Weaving")]
    public class ExternalTest
    {
        [Test]
        public void ExternalAdviceTest()
        {
            var emptyExternalAdvisedClass = new EmptyExternalAdvisedClass();
            emptyExternalAdvisedClass.MethodTest();
        }

        [Test]
        public void ExternalWeavingAdviceTest()
        {
            var emptyExternalAdvisedClass = new EmptyExternalAdvisedClass();
            emptyExternalAdvisedClass.WeavingAdvisedMethodTest();
        }
    }
}
