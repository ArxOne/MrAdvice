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
    using NUnit.Framework;

    [TestFixture]
    [Category("Weaving")]
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

        [Test]
        public void AbstractedTargetTest()
        {
            var fields = typeof(Abstracted).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(fields.Length, Is.EqualTo(0));

            var a = new Abstracted();
            a.Value = 12;
            Assert.That(a.Value, Is.EqualTo(12));
        }

        [Test]
        public void NotAbstractedTargetTest()
        {
            var fields = typeof(NotAbstracted).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(fields.Length, Is.Not.Zero);
        }
    }
}