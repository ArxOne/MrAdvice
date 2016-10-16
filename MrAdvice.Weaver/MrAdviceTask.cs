#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        return Run(new MrAdviceTask(), LoadArgs(args));
    }

    private static string[] LoadArgs(string[] args)
    {
        if (args.Length == 1 && args[0].StartsWith("@"))
        {
            using (var fileStream = File.OpenText(args[0].Substring(1)))
            {
                var cwd = fileStream.ReadLine();
                Directory.SetCurrentDirectory(cwd);
                var lines = new List<string>();
                for (;;)
                {
                    var line = fileStream.ReadLine();
                    if (line == null)
                        break;
                    if (line == "")
                        continue;
                    lines.Add(line);
                }
                return lines.ToArray();
            }
        }
        return args;
    }
}
