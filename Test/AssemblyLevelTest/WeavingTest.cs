#region Mr. Advice

// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php

#endregion

namespace AssemblyLevelTest
{
    using System.Threading.Tasks;

    class WeavingTest
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<bool> Test()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return true;
        }

        public void F()
        {
            var res = Task.Run(async () => await Test()).Result;
        }
    }
}
