namespace DotGet.Core.Logging
{
    public interface ILogger
    {
        LogLevel Level { get; set; }
        void LogSuccess(string message);
        void LogVerbose(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}