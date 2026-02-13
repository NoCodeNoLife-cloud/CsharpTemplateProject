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
    public bool LogParameters { get; set; }
    public bool LogReturnValue { get; set; }
    public bool LogExecutionTime { get; set; }

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

        LogMethodEntry(methodName, context);

        try
        {
            context.Proceed(); // Execute the original method
            LogMethodExit(methodName, startTime, context, null);
        }
        catch (Exception ex)
        {
            // Check if this exception was already logged in this call chain
            if (!RecentlyLoggedExceptions.Contains(ex))
            {
                LogMethodExit(methodName, startTime, context, ex);
                RecentlyLoggedExceptions.Add(ex);
            }

            LogMethodExit(methodName, startTime, context, ex);
            RecentlyLoggedExceptions.Add(ex);

            throw;
        }
    }

    private void LogMethodEntry(string methodName, MethodAdviceContext context)
    {
        var logMessage = BuildStartLogMessage(methodName);

        if (LogParameters && context.Arguments?.Count > 0)
        {
            var paramInfo = BuildParameterInfo(context.TargetMethod, context.Arguments.ToArray());
            logMessage += $", Parameters: {paramInfo}";
        }

        LoggingServiceImpl.InstanceVal.Log(LogLevel, logMessage);
    }

    private void LogMethodExit(string methodName, DateTimeOffset startTime, MethodAdviceContext context, Exception? exception)
    {
        var executionTime = DateTimeOffset.Now - startTime;
        var isSuccess = exception == null;

        var baseMessage = isSuccess
            ? BuildSuccessLogMessage(methodName)
            : BuildFailureLogMessage(methodName);
        var timeInfo = LogExecutionTime ? $", Execution time: {executionTime.TotalMilliseconds:F2} ms" : string.Empty;
        var returnValueInfo = BuildReturnValueInfo(context);
        var errorMessage = exception != null ? $", Error message: {exception.Message}" : string.Empty;

        var logMessage = $"{baseMessage}{timeInfo}{returnValueInfo}{errorMessage}";

        if (isSuccess)
        {
            LoggingServiceImpl.InstanceVal.Log(LogLevel, logMessage);
        }
        else
        {
            LoggingServiceImpl.InstanceVal.LogError(logMessage, exception!);

            // Clean up the exception tracking after logging the top-level method
            // This ensures we don't accumulate exceptions indefinitely
            if (context.TargetMethod.Name == "Main")
            {
                CleanupRecentlyLoggedExceptions();
            }
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