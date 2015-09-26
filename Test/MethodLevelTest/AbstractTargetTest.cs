#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System.Linq;
    using System.Reflection;
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AbstractTargetTest
    {
        public class Abstracted
        {
            [DeferredStorage]
            public int Value { get; set; }
        }

        public class NotAbstracted
        {
            [DeferredStorage]
            public int Value { get; set; } = 12;
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void AbstractedTargetTest()
        {
            var fields = typeof(Abstracted).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.AreEqual(0, fields.Length);
            var a = new Abstracted();
            a.Value = 12;
            Assert.AreEqual(12, a.Value);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void NotAbstractedTargetTest()
        {
            var fields = typeof(NotAbstracted).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.AreNotEqual(0, fields.Count());
        }
    }
}
