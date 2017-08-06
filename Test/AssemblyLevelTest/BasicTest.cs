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
    public async void StartTask1()
    {
        long r = await AddWhatever();
    }


    public async System.Threading.Tasks.Task<long> AddWhatever()
    {
        return -1;
    }
}
