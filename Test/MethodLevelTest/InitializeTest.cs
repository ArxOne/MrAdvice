
namespace MethodLevelTest
{
    using System.Linq;
    using System.Reflection;
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InitializeTest
    {
        [RecordProperties]
        public int Property { get; set; }

        [TestMethod]
        [TestCategory("Initialize")]
        [RecordMethods]
        public void RecordMethodTest()
        {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
            var methodInfos = RecordMethods.MethodInfos;
            Assert.IsTrue(methodInfos.Any(m => m.Name == "RecordMethodTest" && m.DeclaringType == currentMethod.DeclaringType));
        }

        [TestMethod]
        [TestCategory("Initialize")]
        public void RecordPropertyTest()
        {
            var propertyInfos = RecordProperties.PropertyInfos;
            Assert.IsTrue(propertyInfos.Any(p => p.Name == "Property"));
        }
    }
}
