using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Contracts.Interfaces
{
    public interface IPerformanceTimer
    {
        double ElapsedMicroSeconds { get; }
        double ElapsedMilliSeconds { get; }
        double ElapsedNanoSeconds { get; }
        double ElapsedSeconds { get; }
        IPerformanceTimer Start();
        IPerformanceTimer Stop();
    }
}
