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
    [Category("Info advice")]
    public class InitializeTest
    {
        [RecordProperties]
        public int Property { get; set; }

        [Test]
        [RecordMethods]
        public void RecordMethodTest()
        {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
            var methodInfos = RecordMethods.MethodInfos;

            Assert.That(methodInfos.Any(m => m.Name == "RecordMethodTest"), Is.True);
        }

        [Test]
        public void RecordPropertyTest()
        {
            var propertyInfos = RecordProperties.PropertyInfos;

            Assert.That(propertyInfos.Any(p => p.Name == "Property"), Is.True);
        }
    }
}