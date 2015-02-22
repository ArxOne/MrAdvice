#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace MethodLevelTest
{
    public interface IAdvisedInterface
    {
        int Add(int a, int b);
    }

    internal class AdvisedInterface : IAdvisedInterface
    {
        public int Add(int a, int b)
        {
            throw new System.NotImplementedException();
        }
    }
}
