#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;
    using ArxOne.MrAdvice.Annotation;

    [DynamicHandle]
    public interface IDynamicHandledInterface
    {
        void Nop();
    }

    public interface IDynamicHandledBaseInterface
    {
        void A();
    }

    [DynamicHandle]
    public interface IDynamicHandledInheritedInterface: IDynamicHandledBaseInterface
    {
        void B();
    }
}
