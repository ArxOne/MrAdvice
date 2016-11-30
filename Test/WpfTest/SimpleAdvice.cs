#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace WpfTest
{
    using System.Threading.Tasks;
    using ArxOne.MrAdvice.Advice;

    public class SimpleAdvice : IMethodAsyncAdvice
    {
        public async Task Advise(MethodAsyncAdviceContext context)
        {
            await context.ProceedAsync();
        }
    }
}
