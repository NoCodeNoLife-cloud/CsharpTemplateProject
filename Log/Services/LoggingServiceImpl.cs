using LoggingService.Enums;
using LoggingService.Interfaces;

namespace LoggingService.Services;

public class LoggingServiceImpl : ILoggingService
{
    private static readonly Lazy<LoggingServiceImpl> _instance = new(() => new LoggingServiceImpl());

    private static readonly Dictionary<LogLevel, ConsoleColor> LogColors = new()
    {
        [LogLevel.Information] = ConsoleColor.Green,
        [LogLevel.Warning] = ConsoleColor.Yellow,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Debug] = ConsoleColor.DarkGray,
        [LogLevel.Critical] = ConsoleColor.Magenta
    };

    private LoggingServiceImpl()
    {
    }

    public static LoggingServiceImpl Instance => _instance.Value;

    public void LogInformation(string message)
    {
        WriteColoredLog(LogLevel.Information, message);
    }

    public void LogWarning(string message)
    {
        WriteColoredLog(LogLevel.Warning, message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            var fullMessage = $"{message} - Exception: {exception.Message}";
            WriteColoredLog(LogLevel.Error, fullMessage);
        }
        else
        {
            WriteColoredLog(LogLevel.Error, message);
        }
    }

    public void LogDebug(string message)
    {
        WriteColoredLog(LogLevel.Debug, message);
    }

    public void LogCritical(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            var fullMessage = $"{message} - Exception: {exception.Message}";
            WriteColoredLog(LogLevel.Critical, fullMessage);
        }
        else
        {
            WriteColoredLog(LogLevel.Critical, message);
        }
    }

    private static void WriteColoredLog(LogLevel level, string message)
    {
        var originalColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = LogColors[level];
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt");
            var threadId = Environment.CurrentManagedThreadId;
            var callerInfo = GetCallerInfo();

            Console.WriteLine($"[{level.ToString().ToUpper()}] {timestamp} [{threadId}] {callerInfo} - {message}");
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }

    private static string GetCallerInfo()
    {
        var stackFrame = new System.Diagnostics.StackTrace(3).GetFrame(0); // 调整调用层级
        if (stackFrame != null)
        {
            var method = stackFrame.GetMethod();
            return $"{method?.DeclaringType?.Name}.{method?.Name}";
        }

        return "Unknown";
    }
}