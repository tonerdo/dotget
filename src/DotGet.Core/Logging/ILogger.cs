namespace DotGet.Core.Logging
{
    public interface ILogger
    {
        void LogSuccess(string data);
        void LogVerbose(string data);
        void LogInformation(string data);
        void LogWarning(string data);
        void LogError(string data);
    }
}