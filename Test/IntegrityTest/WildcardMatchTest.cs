using ArxOne.MrAdvice.Pointcut;
using NUnit.Framework;

[TestFixture]
[Category("WildcardMatch")]
public class WildcardMatchTest
{
    [TestCase("abc", "abc", ExpectedResult = true)]
    [TestCase("def", "ghi", ExpectedResult = false)]
    [TestCase("jk?", "jkl", ExpectedResult = true)]
    [TestCase("m*", "mno", ExpectedResult = true)]
    [TestCase("p@r", "pqr", ExpectedResult = true)]
    [TestCase("p@", "p.r", ExpectedResult = false)]
    [TestCase("p@", "pq.", ExpectedResult = false)]
    [TestCase("st!", "stu", ExpectedResult = true)]
    [TestCase("st!", "st.", ExpectedResult = false)]
    [TestCase("*.<@>d@.@", "TestAsync.Program.Test.<<StartTask>b__0_0>d.MoveNext″", ExpectedResult = true)]
    public bool Tests(string pattern, string input)
    {
        return PointcutSelectorRule.WildcardMatch(pattern, input);
    }
}