using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Interfaces;

namespace CustomSerilogImpl.InstanceVal.Service.Services;

/// <summary>
/// Factory class for creating and managing different logging service implementations
/// </summary>
public static class LoggingFactory
{
    private static volatile ILoggingService? _currentInstance;
    private static readonly Lock LockObject = new();

    /// <summary>
    /// Available logging service implementations
    /// </summary>
    public enum LoggingImplementation
    {
        /// <summary>
        /// Custom implementation with colored console output
        /// </summary>
        Custom,

        /// <summary>
        /// Serilog implementation with advanced features
        /// </summary>
        Serilog
    }

    /// <summary>
    /// Gets or sets the current logging implementation type
    /// </summary>
    public static LoggingImplementation CurrentImplementation { get; set; } = LoggingImplementation.Serilog;

    /// <summary>
    /// Gets the singleton instance of the current logging service
    /// </summary>
    public static ILoggingService Instance
    {
        get
        {
            if (_currentInstance != null) return _currentInstance;
            lock (LockObject)
            {
                if (_currentInstance == null)
                {
                    _currentInstance = CreateInstance(CurrentImplementation);
                }
            }

            return _currentInstance;
        }
    }

    /// <summary>
    /// Switches to a different logging implementation
    /// </summary>
    /// <param name="implementation">The implementation to switch to</param>
    /// <param name="preserveLogLevel">Whether to preserve the current log level setting</param>
    public static void SwitchImplementation(LoggingImplementation implementation, bool preserveLogLevel = true)
    {
        lock (LockObject)
        {
            var oldLogLevel = LogLevel.Information;

            // Preserve current log level if requested
            if (preserveLogLevel && _currentInstance != null)
            {
                oldLogLevel = _currentInstance.MinimumLogLevel;
            }

            // Create new instance
            _currentInstance = CreateInstance(implementation);

            // Restore log level
            if (preserveLogLevel)
            {
                _currentInstance.MinimumLogLevel = oldLogLevel;
            }

            CurrentImplementation = implementation;
        }
    }

    /// <summary>
    /// Creates a new instance of the specified logging implementation
    /// </summary>
    /// <param name="implementation">The implementation type to create</param>
    /// <returns>New logging service instance</returns>
    private static ILoggingService CreateInstance(LoggingImplementation implementation)
    {
        return implementation switch
        {
            LoggingImplementation.Custom => LoggingServiceImpl.InstanceVal,
            LoggingImplementation.Serilog => CustomSerilogImpl.InstanceVal,
            _ => LoggingServiceImpl.InstanceVal // Default fallback
        };
    }

    /// <summary>
    /// Resets the factory to use the default implementation
    /// </summary>
    public static void ResetToDefault()
    {
        SwitchImplementation(LoggingImplementation.Custom);
    }

    /// <summary>
    /// Gets descriptive name of current implementation
    /// </summary>
    /// <returns>Implementation name</returns>
    public static string GetCurrentImplementationName()
    {
        return CurrentImplementation switch
        {
            LoggingImplementation.Custom => "Custom Logging Service",
            LoggingImplementation.Serilog => "Serilog Implementation",
            _ => "Unknown Implementation"
        };
    }
}