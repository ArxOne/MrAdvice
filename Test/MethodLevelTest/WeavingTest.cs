#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace MethodLevelTest
{
    using System;
    using System.Runtime.InteropServices;
    using ArxOne.MrAdvice.Advice;

    // https://github.com/ArxOne/MrAdvice/issues/32
    public class Test
    {
        [MyProudAdvice] // this will pass
        public void Method1(FooClass fooClass)
        {
        }

        [MyProudAdvice] // this will pass
        public void Method1A(FooClass foo1, FooClass foo2)
        {
        }

        [MyProudAdvice] // fody error
        public void Method2(FooClass fooClass, string name, long id, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method3(string name, FooClass fooClass, long id, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method4(string name, long id, FooClass fooClass, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method5(string name, long id, long count, FooClass fooClass)
        {
        }

        [MyProudAdvice] // fody error
        public void Method6(FooStruct fooStruct, string name, long id, long count)
        {
        }

        [MyProudAdvice] // fody error
        public void Method6(FooEnum fooEnum, string name, long id, long count)
        {
        }
    }

    public enum FooEnum
    {

    }

    public struct FooStruct
    {
        public int Id { get; set; }
    }

    public class FooClass
    {
        public int Id { get; set; }
    }

    public class MyProudAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            // do things you want here
            context.Proceed(); // this calls the original method
            // do other things here
        }
    }

    public interface ISomething<TValue>
    {
    }

    [MyProudAdvice]
    public class Something : ISomething<int>
    { }
}
