#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace TestApplication
{
    using System;
    using ArxOne.MrAdvice.Advice;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var sc = new SomeClass();
            var c = sc.Add(2, 3);
            sc.Nop();

            sc.DoGeneric<int>();
            var z = new SomeGenericClass<string>();
            z.Do();
        }
    }

    public class SomeClass
    {
        //[SomeAdvice]
        public int Add(int a, int b)
        {
            var c = a + b;
            return c;
        }

        //[SomeAdvice]
        public void Nop()
        {
            int a = 1;
            F(a);
            int b = 12;
            F(b);
            var c = a + b;
            F(c);
        }

        private void F(object c)
        { }

        [SomeOtherAdvice]
        public void DoGeneric<TAnything>()
        {
            var t = typeof(TAnything);
        }
    }

    public class SomeGenericClass<TSomething>
    {
        [SomeOtherAdvice]
        public void Do()
        {
            var t = typeof(TSomething);
        }
    }

    public class SomeAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }

    public class SomeOtherAdvice : Attribute, IMethodAdvice
    {
        public void Advise(MethodAdviceContext context)
        {
            context.Proceed();
        }
    }
}
