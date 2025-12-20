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
    using NUnit.Framework;

    [TestFixture]
    [Category("Integrity")]
    public class TargetFrameworkTest
    {
        [Test]
        public void StandardNETFramework()
        {
            var t = new TargetFramework(".NETFramework,Version=v4.0");

            Assert.That(t.NetFramework, Is.EqualTo(new Version(4, 0)));
            Assert.That(t.Silverlight, Is.Null);
            Assert.That(t.WindowsPhone, Is.Null);
        }

        [Test]
        public void PCL5()
        {
            var t = new TargetFramework(".NETPortable,Version=v4.0,Profile=Profile14");

            Assert.That(t.NetFramework, Is.EqualTo(new Version(4, 0)));
            Assert.That(t.Silverlight, Is.EqualTo(new Version(5, 0)));
            Assert.That(t.WindowsPhone, Is.Null);
        }

        [Test]
        public void Literal45()
        {
            var t = new TargetFramework(".NETFramework,Version=v4.5");

            Assert.That(t.ToString(), Is.EqualTo(".NET Framework 4.5"));
        }
    }
}