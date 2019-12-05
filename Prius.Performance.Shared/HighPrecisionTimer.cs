using System;
using System.Runtime.InteropServices;

namespace Prius.Performance.Shared
{
    public class HighPrecisionTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out Int64 counter);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out Int64 frequency);

        private static readonly Int64 Frequency;

        static HighPrecisionTimer()
        {
            QueryPerformanceFrequency(out Frequency);
        }

        public static Int64 Ticks
        {
            get
            {
                QueryPerformanceCounter(out var counter);
                return counter;
            }
        }

        public static double TicksToSeconds(long ticks)
        {
            return ((Double)ticks) / Frequency;
        }

        public static double TicksToMilliseconds(long ticks)
        {
            return 1000d * ticks / Frequency;
        }

        public static double TicksToMicroseconds(long ticks)
        {
            return 1000000d * ticks / Frequency;
        }

        public static double ElapsedSeconds(Int64 start, Int64 end)
        {
            return TicksToSeconds(end - start);
        }

        public static double ElapsedMilliseconds(Int64 start, Int64 end)
        {
            return TicksToMilliseconds(end - start);
        }

        public static double ElapsedMicroseconds(Int64 start, Int64 end)
        {
            return TicksToMicroseconds(end - start);
        }
    }
}
