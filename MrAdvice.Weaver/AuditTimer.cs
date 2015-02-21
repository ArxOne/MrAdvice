#region Mr. Advice
// Mr. Advice
// A simple post build weaving package
// http://mradvice.arxone.com/
// Released under MIT license http://opensource.org/licenses/mit-license.php
#endregion
namespace ArxOne.MrAdvice
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Audit timer, this is used to measure our weaving performance
    /// Because we WANT performance!
    /// </summary>
    internal class AuditTimer
    {
        private readonly IDictionary<int, IList<Tuple<string, long>>> _zones = new Dictionary<int, IList<Tuple<string, long>>>();

        [ThreadStatic]
        private static Stopwatch _stopwatch;
        private static Stopwatch Stopwatch
        {
            get
            {
                if (_stopwatch == null)
                {
                    _stopwatch=new Stopwatch();
                    _stopwatch.Start();
                }
                return _stopwatch;
            }
        }

        /// <summary>
        /// Starts a new measuring zone.
        /// </summary>
        /// <param name="name">The name.</param>
        public void NewZone(string name)
        {
            GetCurrentZone().Add(Tuple.Create(name, Stopwatch.ElapsedTicks));
        }

        /// <summary>
        /// A last zone marker (to be used at end of methods, for example).
        /// </summary>
        public void LastZone()
        {
            NewZone(null);
        }

        /// <summary>
        /// Gets the report for all measures done until here.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, TimeSpan> GetReport()
        {
            var report = new Dictionary<string, TimeSpan>();
            lock (_zones)
            {
                foreach (var zone in _zones.Values)
                {
                    for (int index = 0; index < zone.Count - 1; index++)
                    {
                        var name = zone[index].Item1;
                        if (name == null)
                            continue;

                        var ticks = zone[index + 1].Item2 - zone[index].Item2;
                        if (!report.ContainsKey(name))
                            report[name] = TimeSpan.Zero;
                        report[name] += TimeSpan.FromTicks(ticks);
                    }
                }
            }
            return report;
        }

        /// <summary>
        /// Gets the current zone (related to current thread).
        /// </summary>
        /// <returns></returns>
        private IList<Tuple<string, long>> GetCurrentZone()
        {
            lock (_zones)
            {
                IList<Tuple<string, long>> zone;
                var managedThreadId = Thread.CurrentThread.ManagedThreadId;
                if (_zones.TryGetValue(managedThreadId, out zone))
                    return zone;
                _zones[managedThreadId] = zone = new List<Tuple<string, long>>();
                return zone;
            }
        }
    }
}
