#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Threading.Tasks;
using AssemblyLevelTest;
using ExternalAdvices;

[assembly: AssemblyAdvice]
[assembly: MethodInfoAdvice]

namespace AssemblyLevelTest
{
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BasicTest
    {
        public void Advised()
        {
            var method = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("Advised", method.Name);
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void AssemblyAdviceTest()
        {
            Advised();
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void AnonymousClassCtorTest()
        {
            var a = new { A = 1, B = "b" };
            Assert.AreEqual(1, a.A);
            Assert.AreEqual("b", a.B);
        }

        [SelfExcludingAdvice(2)]
        public void IncrementBy2()
        {
        }

        [TestMethod]
        [TestCategory("Weaving")]
        public void MethodOverrideTest()
        {
            SelfExcludingAdvice.counter = 0;
            IncrementBy2();
            Assert.AreEqual(2, SelfExcludingAdvice.counter);
        }

    }
}


public class Pouet
{
    //Fatal error
    public void StartTask()
    {
        var task = Task.Run(async () => await AddWhatever());
        var r = task.Result;
    }

    //Fatal error
#pragma warning disable S3168
    public async void StartTask1()
#pragma warning restore S3168
    {
        long r = await AddWhatever();
    }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async System.Threading.Tasks.Task<long> AddWhatever()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        return -1;
    }
}
