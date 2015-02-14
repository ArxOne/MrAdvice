#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// https://github.com/ArxOne/MrAdvice
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace IntegrityTest
{
    using System.Reflection;
    using ArxOne.MrAdvice;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Introduction;
    using ArxOne.MrAdvice.Weaver;
    using ArxOne.MrAdvice.Weaver.Utility;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BindingTest
    {
        [TestMethod]
        [TestCategory("Integrity")]
        public void AdviceInterfaceNameTest()
        {
            var adviceType = typeof(IAdvice);
            Assert.AreEqual(adviceType.FullName, Binding.AdviceInterfaceName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void InfoAdviceInterfaceNameTest()
        {
            var infoAdviceType = typeof(IInfoAdvice);
            Assert.AreEqual(infoAdviceType.FullName, Binding.InfoAdviceInterfaceName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void IntroducedFieldTypeNameTest()
        {
            var introducedFieldType = typeof(IntroducedField<>);
            Assert.AreEqual(introducedFieldType.FullName, Binding.IntroducedFieldTypeName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void InvocationTypeNameTest()
        {
            var invocationType = typeof(Invocation);
            Assert.AreEqual(invocationType.FullName, Binding.InvocationTypeName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void InvocationProceedMethodMethodNameTest()
        {
            var invocationProceedMethodMethod = ReflectionUtility.GetMethodInfo(() => Invocation.ProceedAdvice(null, null, null, null));
            Assert.AreEqual(invocationProceedMethodMethod.Name, Binding.InvocationProceedAdviceMethodName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void InvocationProcessRuntimeInitializersMethodNameTest()
        {
            var invocationProcessRuntimeInitializersMethod = ReflectionUtility.GetMethodInfo(() => Invocation.ProcessInfoAdvices((Assembly)null));
            Assert.AreEqual(invocationProcessRuntimeInitializersMethod.Name, Binding.InvocationProcessInfoAdvicesMethodName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void IntroducedFieldAttributeNameTest()
        {
            Assert.AreEqual(typeof(IntroducedFieldAttribute).FullName, Binding.IntroducedFieldAttributeName);
        }
    }
}
