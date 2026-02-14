using ArxOne.MrAdvice.Advice;
using LoggingService.Enums;
using LoggingService.Services;

namespace CommonFramework.Aop.Attributes;

/// <summary>
/// Logging advice that provides method execution logging with configurable options
/// Compiled at build time for optimal performance
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LogAttribute : Attribute, IMethodAdvice
{
    public LogLevel LogLevel { get; set; }
    
    // Fine-grained logging control options
    public bool LogMethodEntry { get; set; } = true;        // Method entry logging
    public bool LogMethodExit { get; set; } = true;         // Method exit logging
    public bool LogMethodException { get; set; } = true;    // Method exception logging
    public bool LogParameters { get; set; }                 // Method parameters logging
    public bool LogReturnValue { get; set; }                // Method return value logging
    public bool LogExecutionTime { get; set; }              // Execution time logging

    // Thread-static field to track recently logged exceptions and avoid duplicates
    [ThreadStatic] private static HashSet<Exception>? _recentlyLoggedExceptions;

    private static HashSet<Exception> RecentlyLoggedExceptions
    {
        get
        {
            _recentlyLoggedExceptions ??= [];
            return _recentlyLoggedExceptions;
        }
    }

    // Clean up old exceptions to prevent memory leaks
    private static void CleanupRecentlyLoggedExceptions()
    {
        _recentlyLoggedExceptions?.Clear();
    }

    public void Advise(MethodAdviceContext context)
    {
        var methodName = $"{context.TargetType.Name}.{context.TargetMethod.Name}";
        var startTime = DateTimeOffset.Now;

        // Log method entry
        if (LogMethodEntry)
        {
            LogMethodEntryMessage(methodName, context);
        }

        try
        {
            context.Proceed(); // Execute the original method
            
            // Log successful method exit
            if (LogMethodExit)
            {
                LogMethodExitMessage(methodName, startTime, context, null);
            }
        }
        catch (Exception ex)
        {
            // Log method exception (avoid duplicate logging)
            if (LogMethodException && !RecentlyLoggedExceptions.Contains(ex))
            {
                LogMethodExceptionMessage(methodName, startTime, context, ex);
                RecentlyLoggedExceptions.Add(ex);
            }

            throw;
        }
    }

    private void LogMethodEntryMessage(string methodName, MethodAdviceContext context)
    {
        var logMessage = BuildStartLogMessage(methodName);

        // Only log parameter information when parameter logging is enabled and parameters exist
        if (LogParameters && context.Arguments?.Count > 0)
        {
            var paramInfo = BuildParameterInfo(context.TargetMethod, context.Arguments.ToArray());
            logMessage += $", Parameters: {paramInfo}";
        }

        LoggingServiceImpl.InstanceVal.Log(LogLevel, logMessage);
    }

    private void LogMethodExitMessage(string methodName, DateTimeOffset startTime, MethodAdviceContext context, Exception? exception)
    {
        var executionTime = DateTimeOffset.Now - startTime;
        
        var baseMessage = BuildSuccessLogMessage(methodName);
        var timeInfo = LogExecutionTime ? $", Execution time: {executionTime.TotalMilliseconds:F2} ms" : string.Empty;
        var returnValueInfo = BuildReturnValueInfo(context);

        var logMessage = $"{baseMessage}{timeInfo}{returnValueInfo}";

        LoggingServiceImpl.InstanceVal.Log(LogLevel, logMessage);

        // Clean up exception tracking (when main method exits)
        if (context.TargetMethod.Name == "Main")
        {
            CleanupRecentlyLoggedExceptions();
        }
    }

    private void LogMethodExceptionMessage(string methodName, DateTimeOffset startTime, MethodAdviceContext context, Exception exception)
    {
        var executionTime = DateTimeOffset.Now - startTime;
        
        var baseMessage = BuildFailureLogMessage(methodName);
        var timeInfo = LogExecutionTime ? $", Execution time: {executionTime.TotalMilliseconds:F2} ms" : string.Empty;
        var errorMessage = $", Error message: {exception.Message}";
        var stackTraceInfo = $", Stack trace: {exception.StackTrace?.Split('\n')[0].Trim() ?? "N/A"}";

        var logMessage = $"{baseMessage}{timeInfo}{errorMessage}{stackTraceInfo}";

        LoggingServiceImpl.InstanceVal.LogError(logMessage, exception);

        // Clean up exception tracking (when main method exits)
        if (context.TargetMethod.Name == "Main")
        {
            CleanupRecentlyLoggedExceptions();
        }
    }

    private static string BuildStartLogMessage(string methodName)
    {
        return $"Method {methodName} started execution";
    }

    private static string BuildSuccessLogMessage(string methodName)
    {
        return $"Method {methodName} executed successfully";
    }

    private static string BuildFailureLogMessage(string methodName)
    {
        return $"Method {methodName} execution failed";
    }

    private string BuildReturnValueInfo(MethodAdviceContext context)
    {
        // Only log when return value logging is enabled and return value exists
        if (!LogReturnValue || context.ReturnValue == null)
            return string.Empty;

        return $", Return value: {FormatValue(context.ReturnValue)}";
    }

    private static string BuildParameterInfo(System.Reflection.MethodBase method, object[] arguments)
    {
        if (arguments.Length == 0)
            return string.Empty;

        var methodParameters = method.GetParameters();
        var paramStrings = new string[Math.Min(arguments.Length, methodParameters.Length)];

        for (var i = 0; i < paramStrings.Length; i++)
        {
            var paramName = methodParameters[i].Name ?? "unknown";
            var paramValue = FormatValue(arguments[i]);
            paramStrings[i] = $"{paramName}={paramValue}";
        }

        return string.Join(", ", paramStrings);
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "null",
            string str => $"\"{str}\"",
            _ when value.GetType().IsPrimitive || value is DateTime || value is Guid => value.ToString()!,
            _ => $"[{value.GetType().Name}]"
        };
    }
}