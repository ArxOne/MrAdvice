#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

#pragma warning disable SA1200 // Using directives should be placed correctly
using ArxOne.MrAdvice;
using StitcherBoy;
#pragma warning restore SA1200 // Using directives should be placed correctly

BlobberHelper.Setup();
var result = Stitcher.Run<MrAdviceStitcher>(args);
return result;
