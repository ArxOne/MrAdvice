#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace MethodLevelTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
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

        [TestMethod]
        public void TrySomeInt()
        {
            A.TrySomething(out int a);
            Assert.AreEqual(0, a);
        }

        [TestMethod]
        public void TrySomeDateTime()
        {
            A.TrySomething(out DateTime a);
            Assert.AreEqual(DateTime.MinValue, a);
        }
    }
}
