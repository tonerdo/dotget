namespace DotGet.Core.Logging
{
    public interface ILogger
    {
        LogLevel Level { get; set; }
        void LogSuccess(string data);
        void LogVerbose(string data);
        void LogProgress(string data);
        void LogResult(string data);
        void LogInformation(string data);
        void LogWarning(string data);
        void LogError(string data);
    }
}