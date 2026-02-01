using LoggingService.Enums;

namespace LoggingService.Interfaces;

public interface ILoggingService
{
    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="message">Informational message to log</param>
    void LogInformation(string message);
    
    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="message">Warning message to log</param>
    void LogWarning(string message);
    
    /// <summary>
    /// Logs an error message with optional exception details
    /// </summary>
    /// <param name="message">Error message to log</param>
    /// <param name="exception">Optional exception object containing error details</param>
    void LogError(string message, Exception? exception = null);
    
    /// <summary>
    /// Logs a debug message
    /// </summary>
    /// <param name="message">Debug message to log</param>
    void LogDebug(string message);
    
    /// <summary>
    /// Logs a critical message with optional exception details
    /// </summary>
    /// <param name="message">Critical message to log</param>
    /// <param name="exception">Optional exception object containing error details</param>
    void LogCritical(string message, Exception? exception = null);
    
    /// <summary>
    /// Logs a message with specified log level and optional exception details
    /// </summary>
    /// <param name="level">Log level indicating severity of the message</param>
    /// <param name="message">Message to log</param>
    /// <param name="exception">Optional exception object containing error details</param>
    void Log(LogLevel level, string message, Exception? exception = null);
}