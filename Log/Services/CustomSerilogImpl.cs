using System.Diagnostics;
using CustomSerilogImpl.InstanceVal.Service.Enums;
using CustomSerilogImpl.InstanceVal.Service.Interfaces;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace CustomSerilogImpl.InstanceVal.Service.Services;

/// <summary>
/// Custom Serilog implementation that replicates the exact same output format as LoggingServiceImpl
/// </summary>
public class CustomSerilogImpl : ILoggingService
{
    private static readonly Lazy<CustomSerilogImpl> Instance = new(() => new CustomSerilogImpl());
    private readonly ILogger _logger;
    private readonly LoggingLevelSwitch _levelSwitch;

    public CustomSerilogImpl()
    {
        _levelSwitch = new LoggingLevelSwitch(MapToSerilogLevel(LogLevel.Debug));

        _logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_levelSwitch)
            .Enrich.WithThreadId()
            .Enrich.With<CallerInfoEnricher>()
            .WriteTo.Console(
                outputTemplate: "[{Level:u}] {Timestamp:yyyy-MM-dd hh:mm:ss tt} [{ThreadId}] {CallerInfo} - {Message:lj}{NewLine}{Exception}",
                theme: CreateCustomConsoleTheme())
            .CreateLogger();
    }

    public static CustomSerilogImpl InstanceVal => Instance.Value;

    public LogLevel MinimumLogLevel
    {
        get => MapFromSerilogLevel(_levelSwitch.MinimumLevel);
        set => _levelSwitch.MinimumLevel = MapToSerilogLevel(value);
    }

    public bool IsEnabled(LogLevel level) => level >= MinimumLogLevel;

    public void LogInformation(string message)
    {
        if (IsEnabled(LogLevel.Information))
            _logger.Information("{Message}", message);
    }

    public void LogWarning(string message)
    {
        if (IsEnabled(LogLevel.Warning))
            _logger.Warning("{Message}", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        if (!IsEnabled(LogLevel.Error)) return;
        if (exception != null)
            _logger.Error(exception, "{Message} - Exception: {ExceptionMessage}", message, exception.Message);
        else
            _logger.Error("{Message}", message);
    }

    public void LogDebug(string message)
    {
        if (IsEnabled(LogLevel.Debug))
            _logger.Debug("{Message}", message);
    }

    public void LogCritical(string message, Exception? exception = null)
    {
        if (!IsEnabled(LogLevel.Critical)) return;
        if (exception != null)
            _logger.Fatal(exception, "{Message} - Exception: {ExceptionMessage}", message, exception.Message);
        else
            _logger.Fatal("{Message}", message);
    }

    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (!IsEnabled(level)) return;

        var serilogLevel = MapToSerilogLevel(level);

        if (exception != null)
            _logger.Write(serilogLevel, exception, "{Message} - Exception: {ExceptionMessage}", message, exception.Message);
        else
            _logger.Write(serilogLevel, "{Message}", message);
    }

    private static LogEventLevel MapToSerilogLevel(LogLevel level) => level switch
    {
        LogLevel.Debug => LogEventLevel.Verbose,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Critical => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };

    private static LogLevel MapFromSerilogLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => LogLevel.Debug,
        LogEventLevel.Information => LogLevel.Information,
        LogEventLevel.Warning => LogLevel.Warning,
        LogEventLevel.Error => LogLevel.Error,
        LogEventLevel.Fatal => LogLevel.Critical,
        _ => LogLevel.Information
    };

    public static ConsoleTheme CreateCustomConsoleTheme()
    {
        return new SystemConsoleTheme(new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        {
            [ConsoleThemeStyle.LevelInformation] = new() { Foreground = ConsoleColor.Green },
            [ConsoleThemeStyle.LevelWarning] = new() { Foreground = ConsoleColor.Yellow },
            [ConsoleThemeStyle.LevelError] = new() { Foreground = ConsoleColor.Red },
            [ConsoleThemeStyle.LevelDebug] = new() { Foreground = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.LevelVerbose] = new() { Foreground = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.LevelFatal] = new() { Foreground = ConsoleColor.Magenta }
        });
    }

    private class CallerInfoEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var callerInfo = GetCallerInfo();
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CallerInfo", callerInfo));
        }

        private static string GetCallerInfo()
        {
            var stackTrace = new StackTrace(true);

            StackFrame? targetFrame = null;

            // Intelligent search for user code frames, skipping system and framework code
            for (int i = 4; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame == null) continue;

                var frameFileName = frame.GetFileName();
                // Filter out system framework, Serilog and AOP related files
                if (string.IsNullOrEmpty(frameFileName) ||
                    frameFileName.Contains("Serilog") ||
                    frameFileName.Contains("System.Private.CoreLib") ||
                    frameFileName.Contains("MrAdvice") ||
                    frameFileName.Contains("CustomSerilogImpl")) continue;
                targetFrame = frame;
                break;
            }

            // Fallback mechanism: if no suitable frame is found, use the default 4th frame
            targetFrame ??= stackTrace.GetFrame(4);

            if (targetFrame == null) return "Unknown";

            var fileName = targetFrame.GetFileName();
            var lineNumber = targetFrame.GetFileLineNumber();

            if (string.IsNullOrEmpty(fileName)) return "Unknown";

            // Convert to relative path (consistent with original implementation)
            var pathSegments = fileName.Split(Path.DirectorySeparatorChar);
            var startIndex = Math.Max(0, pathSegments.Length - 3);
            var relativePath = string.Join(Path.DirectorySeparatorChar.ToString(), pathSegments.Skip(startIndex));

            return lineNumber > 0 ? $"at {relativePath}({lineNumber})" : $"at {relativePath}";
        }
    }
}