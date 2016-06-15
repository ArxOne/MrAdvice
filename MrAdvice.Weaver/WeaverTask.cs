#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

namespace ArxOne.MrAdvice
{
    public class WeaverTask : StitcherTask<WeaverStitcher>
    {
        /// <summary>
        /// Entry point for nested process (for isolation).
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static int Main(string[] args) => Run(new WeaverTask(), args);
    }
}
