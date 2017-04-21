namespace DotGet.Core.Logging
{
    internal class NuGetLogger : NuGet.Common.ILogger
    {
        private ILogger _logger;

        public NuGetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogDebug(string data) => _logger.LogVerbose(data);
        public void LogVerbose(string data) => _logger.LogVerbose(data);
        public void LogInformation(string data) => _logger.LogVerbose(data);
        public void LogMinimal(string data) => _logger.LogVerbose(data);
        public void LogWarning(string data) => _logger.LogVerbose(data);
        public void LogError(string data) => _logger.LogVerbose(data);
        public void LogSummary(string data) => _logger.LogVerbose(data);
        public void LogInformationSummary(string data) => _logger.LogVerbose(data);
        public void LogErrorSummary(string data) => _logger.LogVerbose(data);
    }
}