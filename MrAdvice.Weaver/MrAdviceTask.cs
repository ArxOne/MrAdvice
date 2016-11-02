#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using ArxOne.MrAdvice;
using StitcherBoy;

// ReSharper disable once CheckNamespace
public class MrAdviceTask : StitcherTask<MrAdviceStitcher>
{
    /// <summary>
    /// Entry point for nested process (for isolation).
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    public static int Main(string[] args)
    {
        BlobberHelper.Setup();
        return Run(new MrAdviceTask(), args);
    }
}
