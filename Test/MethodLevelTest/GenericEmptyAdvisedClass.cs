#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System.Reflection;
    using Advices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class GenericEmptyAdvisedClass<TValue>
    {
        [EmptyMethodAdvice]
        public void DoSomething()
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("DoSomething", thisMethod.Name);
        }

        [EmptyMethodAdvice]
        public TValue DoSomethingElse(TValue value)
        {
            var thisMethod = MethodBase.GetCurrentMethod();
            Assert.AreNotEqual("DoSomethingElse", thisMethod.Name);
            return value;
        }
    }
}
