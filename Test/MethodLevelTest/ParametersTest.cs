#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
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
        [TestCategory("Parameters")]
        public void ChangeReturnValueMethod()
        {
            Assert.AreEqual(4, GetThree());
        }

        public int Add1([ParameterAdvice] int a, int b)
        {
            return a + b;
        }

        [return: ParameterAdvice]
        public int AddV(int a, int b)
        {
            return a + b;
        }

        public string Add1([ParameterAdvice] string a, string b)
        {
            return a + b;
        }

        public int Add2(int a, [ParameterAdvice] int b)
        {
            return a + b;
        }

        public int AddR1([ParameterAdvice] ref int a, int b)
        {
            return a + b;
        }

        public int AddO1([ParameterAdvice] out int a, int b)
        {
            return a = b;
        }

        [ParameterAdvice]
        public int Add2B(int a, int b)
        {
            return a + b;
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeValueParameterParameter()
        {
            int r = Add1(3, 6);
            Assert.AreEqual(10, r);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeValueParameter2Parameter()
        {
            int r = Add2(3, 6);
            Assert.AreEqual(10, r);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeStringParameterParameter()
        {
            var r = Add1("here", "everywhere");
            Assert.AreEqual("herethereeverywhere", r);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeValueResultParameterParameter()
        {
            var r = AddV(1, 2);
            Assert.AreEqual(6, r);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeValueRefParameterParameter()
        {
            int a = 7;
            var r = AddR1(ref a, 3);
            Assert.AreEqual(8, a);
            Assert.AreEqual(11, r);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void ChangeValueOutParameterParameter()
        {
            int a;
            AddO1(out a, 12);
            Assert.AreEqual(24, a);
        }

        [TestMethod]
        [TestCategory("Parameters")]
        public void AllParametersAtMethodLevel()
        {
            var r = Add2B(5, 8);
            Assert.AreEqual((5 + 1 + 8 + 1) * 2, r);
        }
    }
}
