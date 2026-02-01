using LoggingService.Enums;
using LoggingService.Interfaces;

namespace LoggingService.Services;

/// <summary>
/// Implementation of logging service with colored console output and caller information
/// </summary>
public class LoggingServiceImpl : ILoggingService
{
    private static readonly Lazy<LoggingServiceImpl> Instance = new(() => new LoggingServiceImpl());

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

    /// <summary>
    /// Gets the singleton instance of LoggingServiceImpl
    /// </summary>
    public static LoggingServiceImpl InstanceVal => Instance.Value;

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">The message to log</param>
    public void LogInformation(string message)
    {
        WriteColoredLog(LogLevel.Information, message);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">The warning message to log</param>
    public void LogWarning(string message)
    {
        WriteColoredLog(LogLevel.Warning, message);
    }

    /// <summary>
    /// Logs an error message with optional exception details
    /// </summary>
    /// <param name="message">The error message to log</param>
    /// <param name="exception">Optional exception to include in the log</param>
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

    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">The debug message to log</param>
    public void LogDebug(string message)
    {
        WriteColoredLog(LogLevel.Debug, message);
    }

    /// <summary>
    /// Logs a critical message with optional exception details
    /// </summary>
    /// <param name="message">The critical message to log</param>
    /// <param name="exception">Optional exception to include in the log</param>
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

    /// <summary>
    /// Logs a message with specified log level and optional exception details
    /// </summary>
    /// <param name="level">The log level</param>
    /// <param name="message">The message to log</param>
    /// <param name="exception">Optional exception to include in the log</param>
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (exception != null)
        {
            var fullMessage = $"{message} - Exception: {exception.Message}";
            WriteColoredLog(level, fullMessage);
        }
        else
        {
            WriteColoredLog(level, message);
        }
    }

    /// <summary>
    /// Writes a colored log message to console with timestamp and caller information
    /// </summary>
    /// <param name="level">The log level</param>
    /// <param name="message">The message to log</param>
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

    /// <summary>
    /// Gets caller method information from the call stack
    /// </summary>
    /// <returns>Formatted string containing class name and method name of the caller</returns>
    private static string GetCallerInfo()
    {
        var stackFrame = new System.Diagnostics.StackTrace(3).GetFrame(0);
        if (stackFrame == null) return "Unknown";
        var method = stackFrame.GetMethod();
        return $"{method?.DeclaringType?.Name}.{method?.Name}";
    }
}