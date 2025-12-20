#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System.Reflection;
using System.Threading.Tasks;
using AssemblyLevelTest;
using ExternalAdvices;
using NUnit.Framework;

[assembly: AssemblyAdvice]
[assembly: MethodInfoAdvice]

namespace AssemblyLevelTest
{
    [TestFixture]
    [Category("Weaving")]
    public class BasicTest
    {
        public void Advised()
        {
            var method = MethodBase.GetCurrentMethod();
            Assert.That(method.Name, Is.Not.EqualTo("Advised"));
        }

        [Test]
        public void AssemblyAdviceTest()
        {
            Advised();
        }

        [Test]
        public void AnonymousClassCtorTest()
        {
            var a = new { A = 1, B = "b" };
            Assert.That(a.A, Is.EqualTo(1));
            Assert.That(a.B, Is.EqualTo("b"));
        }

        [SelfExcludingAdvice(2)]
        public void IncrementBy2()
        {
        }

        [Test]
        public void MethodOverrideTest()
        {
            SelfExcludingAdvice.counter = 0;
            IncrementBy2();
            Assert.That(SelfExcludingAdvice.counter, Is.EqualTo(2));
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
