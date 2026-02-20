using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Services;

namespace CustomSerilogImpl.InstanceVal.Service.Configuration;

/// <summary>
/// Configuration class for logging service settings
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets the default logging implementation
    /// </summary>
    public LoggingFactory.LoggingImplementation DefaultImplementation { get; set; } = LoggingFactory.LoggingImplementation.Custom;

    /// <summary>
    /// Gets or sets the minimum log level
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets whether to preserve log level when switching implementations
    /// </summary>
    public bool PreserveLogLevelOnSwitch { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable colored output (applies to custom implementation)
    /// </summary>
    public bool EnableColoredOutput { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include caller information in logs
    /// </summary>
    public bool IncludeCallerInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets the timestamp format for log messages
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-dd hh:mm:ss tt";

    /// <summary>
    /// Applies this configuration to the logging factory
    /// </summary>
    public void Apply()
    {
        LoggingFactory.CurrentImplementation = DefaultImplementation;
        LoggingFactory.Instance.MinimumLogLevel = MinimumLogLevel;
    }

    /// <summary>
    /// Creates a default configuration
    /// </summary>
    /// <returns>Default logging configuration</returns>
    public static LoggingConfiguration CreateDefault()
    {
        return new LoggingConfiguration();
    }

    /// <summary>
    /// Creates a development-friendly configuration
    /// </summary>
    /// <returns>Development logging configuration</returns>
    public static LoggingConfiguration CreateDevelopment()
    {
        return new LoggingConfiguration
        {
            DefaultImplementation = LoggingFactory.LoggingImplementation.Custom,
            MinimumLogLevel = LogLevel.Debug,
            EnableColoredOutput = true,
            IncludeCallerInfo = true
        };
    }

    /// <summary>
    /// Creates a production-friendly configuration
    /// </summary>
    /// <returns>Production logging configuration</returns>
    public static LoggingConfiguration CreateProduction()
    {
        return new LoggingConfiguration
        {
            DefaultImplementation = LoggingFactory.LoggingImplementation.Serilog,
            MinimumLogLevel = LogLevel.Information,
            EnableColoredOutput = false,
            IncludeCallerInfo = true
        };
    }
}