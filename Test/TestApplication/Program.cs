#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

using TestApplication;

//[assembly:TestAdvice]

namespace TestApplication
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using ArxOne.MrAdvice.Advice;
    using ArxOne.MrAdvice.Annotation;
    using ArxOne.MrAdvice.Introduction;
    using ExternalAdvices;
    using MrAdvice.Advice;
#if no
    public class PublicClass
    {
        public void PublicMethod()
        {
        }

        protected void ProtectedMethod()
        {
        }

        private void PrivateMethod()
        {
        }

        internal void InternalMethod()
        {
        }

        protected internal void ProtectedInternalMethod()
        {
        }
    }

    public class Benchmark
    {
        public int Value { get; [SomeAdvice] set; }
    }

    internal class InternalClass
    {
    }

    public class Toot
    {
        public static object F(object i, object[] p)
        {
            return i;
        }

        public void G()
        {
            ProceedDelegate f = F;
        }
    }

    //public class G1<T1>
    //{
    //    public void F1<T2>()
    //    {
    //    }

    //    public void Z<T3>()
    //    {
    //        ProceedDelegate d = new ProceedDelegate(F1<T3>);
    //    }
    //}

    public interface IExternalAdvisedInterface2 : IExternalAdvisedInterface
    {
        //Task<ExternalData[]> I();
    }

    public static class Program1
    {
        public static void Main1(string[] args)
        {
            var a = new SomeAdvice();
            var ii = a.Handle<IExternalAdvisedInterface2>();

            //var b = new Benchmark();
            //var t1 = DateTime.UtcNow;
            //for (int i = 0; i < 1000000; i++)
            //    b.Value++;
            //var dt = DateTime.UtcNow - t1;
            //Console.WriteLine($"Elapsed time={dt}");
            //using (var ts = File.AppendText("Timings.txt"))
            //    ts.WriteLine($"Elapsed time={dt}");

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
        {
        }

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
        public List<int> X;

        public SharedIntroducedField<int> Y;
        public IntroducedField<int> Z;

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
#endif
    class Program
    {
        [TestAdvice]
        static void Main(string[] args)
        {
            //Works
            var tSucceed = new TaskSucceed();
            var task1 = tSucceed.GetTask();
            task1.Start();

            //Works
            var tExcludeAdvice = new TaskExcludeAdvice();
            var task2 = tExcludeAdvice.GetTask();
            task2.Start();

            //Works
            var tExcludePointcutBase = new TaskExcludePointcutBase();
            var task3 = tExcludePointcutBase.GetTask();
            task3.Start();

            //Throws exception
            var tExcludePointcut = new TaskExcludePointcut();
            var task4 = tExcludePointcut.GetTask();
            task4.Start();

            Console.ReadLine();
        }
    }


    [ExcludePointcut("*TaskExcludePointcut*")]
    [ExcludePointcut("*TaskExcludePointcutBase*")]
    [ExcludePointcut("*TaskExcludeAdvice*")]
    public class TestAdvice : BaseAdvice
    {
    }

    [ExcludePointcut("*TaskExcludePointcutBase*")]
    public abstract class BaseAdvice : Attribute, IMethodAdvice
    {
        public virtual void Advise(MethodAdviceContext context)
        {
            Console.WriteLine($"<==========before proceed {context.TargetName}==========>");
            context.Proceed();
            Console.WriteLine("<==========end proceed==========>");
        }
    }

    public class TaskSucceed
    {
        public Task<int> GetTask()
        {
            return new Task<int>(() => 10);
        }
    }

    [ExcludeAdvices("*")]
    public class TaskExcludeAdvice
    {
        public Task<int> GetTask()
        {
            return new Task<int>(() => 10);
        }
    }

    public class TaskExcludePointcutBase
    {
        public Task<int> GetTask()
        {
            return new Task<int>(() => 10);
        }
    }

    public class TaskExcludePointcut
    {
        public Task<int> GetTask()
        {
            return new Task<int>(() => 10);
        }
    }

}