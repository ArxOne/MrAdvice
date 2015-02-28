#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    using System;

    public interface IAdvisedInterface
    {
        event EventHandler SomeEvent;

        int SomeProperty { get; set; }

        void DoNothing();
        int DoSomething(int a, int b);
        void DoSomethingWithRef(ref int a);
        void DoSomethingWithOut(out int a);
    }
}
