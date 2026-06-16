#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion


//[assembly:EmptyAsyncAdvice]


using NUnit.Framework;

namespace DotNetCoreTest
{
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class BasicTest
    {
        [EmptyAsyncAdvice]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<bool> AsyncAdvised()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return true;
        }

        [Test]
        public void TypedTaskTest()
        {
            var t = AsyncAdvised();
            t.Wait();
            var b = t.Result;
            Assert.That(b);
        }

        [Test]
        public void NestedAsyncTest()
        {

            NestedAsync c = new NestedAsync();

            //Here is the first problem to solve (can't build: Couldn't find an instruction, maybe it was removed. It's still being referenced by some code or by the PDB)
            //Task.Run(async () => { await c.TM1Async(); });

            var t = Task.Run(() =>
            {
                try
                {
                    // c1 c = new c1();
                    var r = c.TM2Async();
                    r.Wait();
                    Assert.That(r.Result, Is.EqualTo("OK"));
                }
                catch (Exception /*ex*/)
                {
                    //Console.WriteLine(ex.ToString());
                }


            });
            t.Wait();
        }
    }
}
