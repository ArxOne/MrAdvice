﻿#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using ArxOne.MrAdvice;
using StitcherBoy;

BlobberHelper.Setup();
var result = Stitcher.Run<MrAdviceStitcher>(args);
return result;
