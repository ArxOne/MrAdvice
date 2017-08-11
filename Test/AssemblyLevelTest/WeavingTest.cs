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
        private async Task<bool> Test()
        {
            return true;
        }

        public void F()
        {
            var res = Task.Run(async () => await Test()).Result;
        }
    }
}
