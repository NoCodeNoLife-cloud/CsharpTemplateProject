using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Interfaces;

namespace CustomSerilogImpl.InstanceVal.Service.Services;

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
    /// Gets or sets the minimum log level that will be output
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Checks if the specified log level is enabled
    /// </summary>
    /// <param name="level">Log level to check</param>
    /// <returns>True if the log level is enabled, false otherwise</returns>
    public bool IsEnabled(LogLevel level)
    {
        return level >= MinimumLogLevel;
    }

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">The message to log</param>
    public void LogInformation(string message)
    {
        if (IsEnabled(LogLevel.Information))
        {
            WriteColoredLog(LogLevel.Information, message);
        }
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">The warning message to log</param>
    public void LogWarning(string message)
    {
        if (!IsEnabled(LogLevel.Warning)) return;
        WriteColoredLog(LogLevel.Warning, message);
    }

    /// <summary>
    /// Logs an error message with optional exception details
    /// </summary>
    /// <param name="message">The error message to log</param>
    /// <param name="exception">Optional exception to include in the log</param>
    public void LogError(string message, Exception? exception = null)
    {
        if (!IsEnabled(LogLevel.Error)) return;
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
        if (!IsEnabled(LogLevel.Debug)) return;
        WriteColoredLog(LogLevel.Debug, message);
    }

    /// <summary>
    /// Logs a critical message with optional exception details
    /// </summary>
    /// <param name="message">The critical message to log</param>
    /// <param name="exception">Optional exception to include in the log</param>
    public void LogCritical(string message, Exception? exception = null)
    {
        if (!IsEnabled(LogLevel.Critical)) return;
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
        if (!IsEnabled(level)) return;
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
    /// Gets caller method information from the call stack including line number
    /// </summary>
    /// <returns>Formatted string containing file path and line number in format 'at Client/App/Program.cs(2)'</returns>
    private static string GetCallerInfo()
    {
        var stackTrace = new System.Diagnostics.StackTrace(true);
        var stackFrame = stackTrace.GetFrame(3); // Skip current method, WriteColoredLog, and the calling log method

        if (stackFrame == null) return "Unknown";

        var fileName = stackFrame.GetFileName();
        var lineNumber = stackFrame.GetFileLineNumber();

        if (string.IsNullOrEmpty(fileName))
            return "Unknown";

        // Convert full file path to relative path from workspace root
        Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var relativePath = fileName;

        // Extract the relevant part of the path (remove full system path)
        var pathSegments = fileName.Split(Path.DirectorySeparatorChar);
        if (pathSegments.Length < 2) return lineNumber > 0 ? $"at {relativePath}({lineNumber})" : $"at {relativePath}";
        // Take the last two segments to form a relative path like Client/App/Program.cs
        var startIndex = Math.Max(0, pathSegments.Length - 3);
        relativePath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Skip(startIndex));

        return lineNumber > 0 ? $"at {relativePath}({lineNumber})" : $"at {relativePath}";
    }
}