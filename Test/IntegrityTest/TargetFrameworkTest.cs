#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace IntegrityTest
{
    using System;
    using ArxOne.MrAdvice.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TargetFrameworkTest
    {
        [TestMethod]
        [TestCategory("Integrity")]
        public void StandardNETFramework()
        {
            var t = new TargetFramework(".NETFramework,Version=v4.0");
            Assert.AreEqual(new Version(4, 0), t.Net);
            Assert.IsNull(t.Silverlight);
            Assert.IsNull(t.WindowsPhone);
        }

        [TestMethod]
        [TestCategory("Integrity")]
        public void PCL5()
        {
            var t = new TargetFramework(".NETPortable,Version=v4.0,Profile=Profile14");
            Assert.AreEqual(new Version(4, 0), t.Net);
            Assert.AreEqual(new Version(5, 0), t.Silverlight);
            Assert.IsNull(t.WindowsPhone);
        }

        [TestMethod]
        [TestCategory("Integrity")]
        public void Literal45()
        {
            var t = new TargetFramework(".NETFramework,Version=v4.5");
            Assert.AreEqual(t.ToString(), ".NET 4.5");
        }
    }
}
