#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RenamingTest
    {
        public class Renaming : Attribute
        {

        }

        [Renaming]
        public class Something : Attribute, IAdvice
        { }

        public class Target
        {
            [Something]
            public void M()
            { }

            public void F()
            {
                M();
            }
        }

        [TestMethod]
        [TestCategory("Renaming")]
        public void SimpleTest()
        { }
    }
}
