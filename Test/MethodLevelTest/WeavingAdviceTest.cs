#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using System.Reflection;
    using ArxOne.MrAdvice.Advice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WeavingAdviceTest
    {
        public class MethodWeavingAdvice : Attribute, IMethodWeavingAdvice
        {
            public void Advise(MethodWeavingContext context)
            {
                context.AddPublicAutoProperty(context.TargetMethodName + "_Friend", typeof(string));
                context.TargetMethodName += "_Renamed";
            }
        }

        public class WeavingAdvisedClass
        {
            //public string CompilerAutoProperty { get; set; }

            [MethodWeavingAdvice]
            public void WeavingAdvisedMethod()
            {
                var newProperty = GetType().GetProperty("WeavingAdvisedMethod_Friend");
                Assert.IsNotNull(newProperty);
                var thisMethod = MethodBase.GetCurrentMethod();
                Assert.IsTrue(thisMethod.Name.StartsWith("WeavingAdvisedMethod_Renamed"));
            }
        }

        [TestMethod]
        [TestCategory("Weaving advice")]
        public void SimpleWeavingAdviceTest()
        {
            var c = new WeavingAdvisedClass();
            c.WeavingAdvisedMethod();
        }
    }
}
