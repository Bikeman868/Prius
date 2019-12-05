using System;
using System.Runtime.InteropServices;
using Prius.Contracts.Interfaces.Utility;

namespace Prius.Contracts.Utility
{
    /// <summary>
    /// Provides very high performance time measurement.
    /// Accurate to sub-nanosecond resolution.
    /// Works like a stop watch, can be started and stopped many times and it accumulates elapsed time while running.
    /// Can query elapsed time after it has been stopped, or while it is running.
    /// </summary>
    public class PerformanceTimer : IPerformanceTimer
    {
        #region Static stuff for access to very high precision timing

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out Int64 performanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out Int64 frequency);

        private static Int64 frequency;

        static PerformanceTimer()
        {
            QueryPerformanceFrequency(out frequency);
        }

        public static Int64 TimeNow
        {
            get
            {
                QueryPerformanceCounter(out var startTime);
                return startTime;
            }
        }

        #endregion

        public static double TicksToSeconds(long ticks)
        {
            return ((Double)ticks) / frequency;
        }

        public static long SecondsToTicks(double seconds)
        {
            return (long)(seconds * frequency);
        }

        public static double TicksToMilliseconds(long ticks)
        {
            return 1000d * ticks / frequency;
        }

        public static double TicksToMicroseconds(long ticks)
        {
            return 1000000d * ticks / frequency;
        }

        public static double TicksToNanoseconds(long ticks)
        {
            return 1000000000d * ticks / frequency;
        }

        private Int64 _startTime;
        private Int64 _elapsedTime;
        private bool _running;

        public string MethodName { get; set; }
        public string TimerName { get; set; }

        public Int64 ElapsedTicks { get { return _running ? _elapsedTime + (TimeNow - _startTime) : _elapsedTime; } }
        public Int64 StartTicks { get { return _startTime; } }

        public Double ElapsedSeconds { get { return TicksToSeconds(ElapsedTicks); } }
        public Double ElapsedMilliSeconds { get { return TicksToMilliseconds(ElapsedTicks); } }
        public Double ElapsedMicroSeconds { get { return TicksToMicroseconds(ElapsedTicks); } }
        public Double ElapsedNanoSeconds { get { return TicksToNanoseconds(ElapsedTicks); } }

        public PerformanceTimer()
        {
        }

        public IPerformanceTimer Initialize(Int64 startTicks)
        {
            _startTime = startTicks;
            _running = true;
            return this;
        }

        public PerformanceTimer(string methodName, string timerName)
        {
            MethodName = methodName;
            TimerName = timerName;
        }

        public IPerformanceTimer Start()
        {
            _running = true;
            _startTime = TimeNow;
            return this;
        }

        public IPerformanceTimer Stop()
        {
            if (_running)
            {
                _elapsedTime += TimeNow - _startTime;
                _running = false;
            }
            return this;
        }
    }
}
