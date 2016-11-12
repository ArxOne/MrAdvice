#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace IntegrityTest
{
    using ArxOne.MrAdvice.Pointcut;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WildcardMatchTest
    {
        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void SimpleEquals()
        {
            Assert.IsTrue(PointcutSelectorRule.WildcardMatch("abc", "abc"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void SimpleNotEquals()
        {
            Assert.IsFalse(PointcutSelectorRule.WildcardMatch("def", "ghi"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void QuestionMark()
        {
            Assert.IsTrue(PointcutSelectorRule.WildcardMatch("jk?", "jkl"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void Asterisk()
        {
            Assert.IsTrue(PointcutSelectorRule.WildcardMatch("m*", "mno"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void Arobase()
        {
            Assert.IsTrue(PointcutSelectorRule.WildcardMatch("p@r", "pqr"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ArobaseAndDot()
        {
            Assert.IsFalse(PointcutSelectorRule.WildcardMatch("p@", "p.r"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ArobaseAndFinalDot()
        {
            Assert.IsFalse(PointcutSelectorRule.WildcardMatch("p@", "pq."));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ExclamationMark()
        {
            Assert.IsTrue(PointcutSelectorRule.WildcardMatch("st!", "stu"));
        }

        [TestMethod]
        [TestCategory("WildcardMatch")]
        public void ExclamationMarkAndDot()
        {
            Assert.IsFalse(PointcutSelectorRule.WildcardMatch("st!", "st."));
        }
    }
}
