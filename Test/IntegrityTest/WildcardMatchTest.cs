#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace IntegrityTest
{
    using ArxOne.MrAdvice.Weaver;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WildcardMatchTest
    {
        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void SimpleEquals()
        {
            Assert.IsTrue(PointcutRule.WildcardMatch("abc", "abc"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void SimpleNotEquals()
        {
            Assert.IsFalse(PointcutRule.WildcardMatch("def", "ghi"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void QuestionMark()
        {
            Assert.IsTrue(PointcutRule.WildcardMatch("jk?", "jkl"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void Asterisk()
        {
            Assert.IsTrue(PointcutRule.WildcardMatch("m*", "mno"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void Arobase()
        {
            Assert.IsTrue(PointcutRule.WildcardMatch("p@r", "pqr"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ArobaseAndDot()
        {
            Assert.IsFalse(PointcutRule.WildcardMatch("p@", "p.r"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ArobaseAndFinalDot()
        {
            Assert.IsFalse(PointcutRule.WildcardMatch("p@", "pq."));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ExclamationMark()
        {
            Assert.IsTrue(PointcutRule.WildcardMatch("st!", "stu"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ExclamationMarkAndDot()
        {
            Assert.IsFalse(PointcutRule.WildcardMatch("st!", "st."));
        }
    }
}
