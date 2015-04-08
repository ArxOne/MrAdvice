#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using ExternalAdvices;

[assembly: AssemblyAdvice]

namespace AssemblyLevelTest
{
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BasicTest
    {
        public void Advised()
        {
            var method = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("Advised", method.Name);
        }

        [TestMethod]
        public void AssemblyAdviceTest()
        {
            Advised();
        }
    }
}
