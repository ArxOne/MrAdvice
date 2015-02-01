
namespace MethodLevelTest
{
    using System.Linq;
    using System.Reflection;
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InitializeTest
    {
        [TestMethod]
        [TestCategory("Initialize")]
        [RecordMethods]
        public void RecordMethodTest()
        {
            var currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
            var methodInfos = RecordMethods.MethodInfos;
            Assert.IsTrue(methodInfos.Any(m => m.Name == "RecordMethodTest" && m.DeclaringType == currentMethod.DeclaringType));
        }
    }
}
