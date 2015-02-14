#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
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
        public void StandardNETFrameworkTest()
        {
            var t = new TargetFramework(".NETFramework,Version=v4.0");
            Assert.AreEqual(new Version(4, 0), t.NET);
            Assert.IsNull(t.Silverlight);
            Assert.IsNull(t.WindowsPhone);
        }

        [TestMethod]
        [TestCategory("Integrity")]
        public void PCL5Test()
        {
            var t = new TargetFramework(".NETPortable,Version=v4.0,Profile=Profile14");
            Assert.AreEqual(new Version(4, 0), t.NET);
            Assert.AreEqual(new Version(5, 0), t.Silverlight);
            Assert.IsNull(t.WindowsPhone);
        }
    }
}
