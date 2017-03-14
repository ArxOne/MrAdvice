#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace TestApplication
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using ArxOne.MrAdvice.Advice;

    public class PublicClass
    {
        public void PublicMethod() { }

        protected void ProtectedMethod() { }

        private void PrivateMethod() { }

        internal void InternalMethod() { }

        protected internal void ProtectedInternalMethod() { }
    }

    public class Benchmark
    {

        public int Value { get; [SomeAdvice] set; }
    }

    internal class InternalClass
    { }

    public static class Program
    {
        public static void Main(string[] args)
        {
            var b = new Benchmark();
            var t1 = DateTime.UtcNow;
            for (int i = 0; i < 1000000; i++)
                b.Value++;
            var dt = DateTime.UtcNow - t1;
            Console.WriteLine($"Elapsed time={dt}");
            using (var ts = File.AppendText("Timings.txt"))
                ts.WriteLine($"Elapsed time={dt}");

            //var sc = new SomeClass();
            //var c = sc.Add(2, 3);
            //sc.Nop();

            //sc.DoGeneric<int>();
            //var z = new SomeGenericClass<string>();
            //z.Do();
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
