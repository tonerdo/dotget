using System.Threading.Tasks;
using NuGet.Common;

namespace DotGet.Core.Logging
{
    internal class NuGetLogger : NuGet.Common.ILogger
    {
        private ILogger _logger;

        public NuGetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(LogLevel level, string data) => _logger.LogVerbose(data);

        public void Log(ILogMessage message) => Log(message.Level, message.Message);

        public Task LogAsync(LogLevel level, string data)
            => Task.Run(() => { _logger.LogVerbose(data); });

        public Task LogAsync(ILogMessage message) => LogAsync(message.Level, message.Message);

        public void LogDebug(string data) => _logger.LogVerbose(data);

        public void LogError(string data) => _logger.LogVerbose(data);

        public void LogInformation(string data) => _logger.LogVerbose(data);

        public void LogInformationSummary(string data) => _logger.LogVerbose(data);

        public void LogMinimal(string data) => _logger.LogVerbose(data);

        public void LogVerbose(string data) => _logger.LogVerbose(data);

        public void LogWarning(string data) => _logger.LogVerbose(data);
    }
}