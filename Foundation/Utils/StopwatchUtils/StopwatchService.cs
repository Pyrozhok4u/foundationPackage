using System.Collections.Generic;
using System.Diagnostics;
using Debugger = Foundation.Logger.Debugger;

namespace Foundation.Utils.StopwatchUtils
{
    public class StopwatchService
    {
        private static Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();
        
        /// <summary>
        /// Starts a new stopwatch with the given key
        /// </summary>
        /// <param name="key"></param>
        [Conditional(StopwatchConfig.StopwatchSymbol)]
        public static void Start(string key)
        {
            if (_stopwatches.ContainsKey(key))
            {
                Debugger.LogError(null, $"Stop watch already contains a timer with key {key}");
                return;
            }
            Stopwatch stopwatch = Stopwatch.StartNew();
            _stopwatches.Add(key, stopwatch);
        }

        /// <summary>
        /// Returns the elapsed time in ms of the given timer key (0 if doesn't exists)
        /// </summary>
        public static long ElapsedMilliseconds(string key)
        {
            long elapsedMilliseconds = 0;
            #if ENABLE_STOPWATCH
            if (_stopwatches.TryGetValue(key, out Stopwatch stopwatch))
            {
                elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            }
            #endif
            return elapsedMilliseconds;
        }
        
        /// <summary>
        /// Stops the given stopwatch and returns the elapsed time in ms (0 if doesn't exists)
        /// </summary>
        public static long Stop(string key)
        {
            long elapsedMilliseconds = 0;
            #if ENABLE_STOPWATCH
            if (_stopwatches.TryGetValue(key, out Stopwatch stopwatch))
            {
                elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();
                _stopwatches.Remove(key);
            }
            #endif

            return elapsedMilliseconds;
        }
        
    }
}
