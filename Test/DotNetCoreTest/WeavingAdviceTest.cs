﻿using System;
using ArxOne.MrAdvice.Advice;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCoreTest
{
    [TestClass]
    public class WeavingAdviceTest
    {
        public class Disposer : Attribute, ITypeWeavingAdvice
        {
            public void Advise(WeavingContext context)
            {
                var disposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
                //var disposeMethod = typeof(AutoDispose).GetMethod(nameof(IDisposable.Dispose));
                Action<object> disposeSurrogate = DisposeSurrogate;
                context.TypeWeaver.After(disposeMethod, disposeSurrogate);
            }

            public static void DisposeSurrogate(object target)
            {
                var ad = (AutoDispose)target;
                ad.Disposed++;
            }
        }

        public interface IAutoDispose : IDisposable
        {
            void IDisposable.Dispose() { }
        }

        [Disposer]
        public class AutoDispose : IAutoDispose
        {
            public int Disposed;
        }

        [TestMethod]
        public void OverrideDisposeTest()
        {
            var ad = new AutoDispose();
            using (ad) { }
            Assert.AreEqual(1, ad.Disposed);
        }
    }
}
