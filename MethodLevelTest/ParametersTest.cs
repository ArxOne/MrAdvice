#region Weavisor
// Arx One Aspects
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Release under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Summary description for ParametersTest
    /// </summary>
    [TestClass]
    public class ParametersTest
    {
        [ChangeParameter(NewParameter = 2)]
        public void TakeTwo(int oneIsTwo)
        {
            Assert.AreEqual(2, oneIsTwo);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeParameterMethod()
        {
            TakeTwo(1);
        }

        [ChangeParameter(NewReturnValue = 4)]
        public int GetThree()
        {
            return 3;
        }

        [TestMethod]
        [TestCategory("ReturnValue")]
        public void ChangeReturnValueMethod()
        {
            Assert.AreEqual(4, GetThree());
        }
    }
}
