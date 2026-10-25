using System.Diagnostics;

namespace AuthenticationModule.Decorators
{
    public interface ILoggingService
    {
        Task LogAsync(string action, string user, string details, LogLevel level);
    }

    public class BaseLoggingService : ILoggingService
    {
        public Task LogAsync(string action, string user, string details, LogLevel level) => Task.CompletedTask;
    }

    public class LoggingDecorator : ILoggingService
    {
        private readonly ILogger<LoggingDecorator> _logger;
        public LoggingDecorator(ILogger<LoggingDecorator> logger) => _logger = logger;

        public async Task LogAsync(string action, string user, string details, LogLevel level)
        {
            _logger.Log(level, "{Action} by {User}: {Details} [{Machine}/{Pid}]",
                action, user, details, Environment.MachineName, Process.GetCurrentProcess().Id);
            await Task.CompletedTask; // aquí iría el INSERT a una tabla logs (opcional)
        }
    }
}