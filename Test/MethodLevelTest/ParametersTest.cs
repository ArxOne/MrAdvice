#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using Advices;
    using NUnit.Framework;

    /// <summary>
    /// Summary description for ParametersTest
    /// </summary>
    [TestFixture]
    [Category("Parameters")]
    public class ParametersTest
    {
        [ChangeParameter(NewParameter = 2)]
        public void TakeTwo(int oneIsTwo)
        {
            Assert.That(oneIsTwo, Is.EqualTo(2));
        }

        [Test]
        public void ChangeParameterMethod()
        {
            TakeTwo(1);
        }

        [ChangeParameter(NewReturnValue = 4)]
        public int GetThree()
        {
            return 3;
        }

        [Test]
        public void ChangeReturnValueMethod()
        {
            Assert.That(GetThree(), Is.EqualTo(4));
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

        [Test]
        public void ChangeValueParameterParameter()
        {
            int r = Add1(3, 6);
            Assert.That(r, Is.EqualTo(10));
        }

        [Test]
        public void ChangeValueParameter2Parameter()
        {
            int r = Add2(3, 6);
            Assert.That(r, Is.EqualTo(10));
        }

        [Test]
        public void ChangeStringParameterParameter()
        {
            var r = Add1("here", "everywhere");
            Assert.That(r, Is.EqualTo("herethereeverywhere"));
        }

        [Test]
        public void ChangeValueResultParameterParameter()
        {
            var r = AddV(1, 2);
            Assert.That(r, Is.EqualTo(6));
        }

        [Test]
        public void ChangeValueRefParameterParameter()
        {
            int a = 7;
            var r = AddR1(ref a, 3);
            Assert.That(a, Is.EqualTo(8));
            Assert.That(r, Is.EqualTo(11));
        }

        [Test]
        public void ChangeValueOutParameterParameter()
        {
            int a;
            AddO1(out a, 12);
            Assert.That(a, Is.EqualTo(24));
        }

        [Test]
        public void AllParametersAtMethodLevel()
        {
            var r = Add2B(5, 8);
            Assert.That(r, Is.EqualTo((5 + 1 + 8 + 1) * 2));
        }
    }
}