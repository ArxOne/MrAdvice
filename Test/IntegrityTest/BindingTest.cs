#region Weavisor
// Weavisor
// A simple post build weaving package
// https://github.com/ArxOne/Weavisor
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace IntegrityTest
{
    using ArxOne.Weavisor;
    using ArxOne.Weavisor.Advice;
    using ArxOne.Weavisor.Introduction;
    using ArxOne.Weavisor.Weaver;
    using ArxOne.Weavisor.Weaver.Utility;
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
            var invocationProceedMethodMethod = ReflectionUtility.GetMethodInfo(() => Invocation.ProceedMethod(null, null, null, null));
            Assert.AreEqual(invocationProceedMethodMethod.Name, Binding.InvocationProceedMethodMethodName);
        }
        [TestMethod]
        [TestCategory("Integrity")]
        public void InvocationProcessRuntimeInitializersMethodNameTest()
        {
            var invocationProcessRuntimeInitializersMethod = ReflectionUtility.GetMethodInfo(() => Invocation.ProcessInitializers(null));
            Assert.AreEqual(invocationProcessRuntimeInitializersMethod.Name, Binding.InvocationProcessRuntimeInitializersMethodName);
        }
    }
}
