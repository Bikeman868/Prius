namespace Prius.Contracts.Interfaces.Utility
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
