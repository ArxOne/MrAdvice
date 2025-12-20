#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace MethodLevelTest
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    [Category("Out Parameters")]
    public class OutTest
    {
        [AdvicesTest.EmptyAdvice]
        public class A
        {
            public static bool TrySomething<T>(out T t)
            {
                t = default(T);
                return true;
            }
        }

        [Test]
        public void TrySomeInt()
        {
            A.TrySomething(out int a);
            Assert.That(a, Is.EqualTo(0));
        }

        [Test]
        public void TrySomeDateTime()
        {
            A.TrySomething(out DateTime a);
            Assert.That(a, Is.EqualTo(DateTime.MinValue));
        }
    }
}