using AspectCore.DynamicProxy;
using LoggingService.Enums;
using LoggingService.Services;

namespace CommonFramework.Attributes;

/// <summary>
/// Interceptor attribute that provides method execution logging functionality with configurable log levels and parameters.
/// </summary>
/// <remarks>
/// Thread-safety: Instance-safe; each method interception operates independently.<br/>
/// Performance: Minimal overhead with conditional logging enabled.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class LogAttribute : AbstractInterceptorAttribute
{
    /// <summary>
    /// Gets or sets the log level for logging method execution events.
    /// </summary>
    /// <value>The log level; defaults to LogLevel.Debug.</value>
    public LogLevel LogLevel { get; set; } = LogLevel.Debug;
    
    /// <summary>
    /// Gets or sets a value indicating whether to log method parameters.
    /// </summary>
    /// <value>True to log method parameters; otherwise false. Defaults to false.</value>
    public bool LogParameters { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to log method return values.
    /// </summary>
    /// <value>True to log method return values; otherwise false. Defaults to false.</value>
    public bool LogReturnValue { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to log method execution time.
    /// </summary>
    /// <value>True to log execution time; otherwise false. Defaults to false.</value>
    public bool LogExecutionTime { get; set; } = false;

    /// <summary>
    /// Intercepts method invocation and logs execution events with configured parameters.
    /// </summary>
    /// <param name="context">The aspect context containing service method information and parameters.</param>
    /// <param name="next">The delegate representing the next interceptor in the chain.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">
    /// Rethrows any exception that occurs during method execution after logging the failure.
    /// </exception>
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        var methodInfo = context.ServiceMethod;
        var methodName = GetMethodName(methodInfo);

        var startTime = DateTimeOffset.Now;

        LogMethodStart(methodName, methodInfo, context);

        try
        {
            await next(context);

            var executionTime = DateTimeOffset.Now - startTime;

            LogMethodSuccess(methodName, executionTime, context);
        }
        catch (Exception ex)
        {
            var executionTime = DateTimeOffset.Now - startTime;

            LogMethodFailure(methodName, executionTime, ex);

            throw;
        }
    }

    /// <summary>
    /// Constructs a formatted method name string combining declaring type and method name.
    /// </summary>
    /// <param name="methodInfo">The method information to extract name components from.</param>
    /// <returns>A formatted string in the form "DeclaringTypeName.MethodName".</returns>
    private static string GetMethodName(System.Reflection.MethodInfo methodInfo)
    {
        var declaringTypeName = methodInfo.DeclaringType?.Name ?? "Unknown";
        var methodName = methodInfo.Name;
        return $"{declaringTypeName}.{methodName}";
    }

    /// <summary>
    /// Logs the start of method execution with optional parameter information.
    /// </summary>
    /// <param name="methodName">The formatted name of the method being executed.</param>
    /// <param name="methodInfo">The method information for parameter details.</param>
    /// <param name="context">The aspect context containing method parameters.</param>
    private void LogMethodStart(string methodName, System.Reflection.MethodInfo methodInfo, AspectContext context)
    {
        string logMessage;

        if (LogParameters && context.Parameters.Length > 0)
        {
            var paramInfo = BuildParameterInfo(methodInfo, context.Parameters);
            logMessage = $"Method {methodName} starts executing, Parameters: {paramInfo}";
        }
        else
        {
            logMessage = $"Method {methodName} starts executing";
        }

        LoggingServiceImpl.InstanceVal.Log(LogLevel, logMessage);
    }

    /// <summary>
    /// Builds a formatted string representation of method parameters and their values.
    /// </summary>
    /// <param name="methodInfo">The method information to extract parameter names.</param>
    /// <param name="parameters">The array of parameter values passed to the method.</param>
    /// <returns>A formatted string containing parameter names and their values.</returns>
    private static string BuildParameterInfo(System.Reflection.MethodInfo methodInfo, object[] parameters)
    {
        var methodParameters = methodInfo.GetParameters();
        var paramStrings = new List<string>();

        for (int i = 0; i < parameters.Length && i < methodParameters.Length; i++)
        {
            var paramName = methodParameters[i].Name;
            var paramValue = FormatValue(parameters[i]);
            paramStrings.Add($"{paramName}={paramValue}");
        }

        return string.Join(", ", paramStrings);
    }

    /// <summary>
    /// Logs successful method execution with optional return value and execution time information.
    /// </summary>
    /// <param name="methodName">The formatted name of the executed method.</param>
    /// <param name="executionTime">The time elapsed during method execution.</param>
    /// <param name="context">The aspect context containing method return value.</param>
    private void LogMethodSuccess(string methodName, TimeSpan executionTime, AspectContext context)
    {
        var returnValueInfo = "";
        if (LogReturnValue && context.ReturnValue != null)
        {
            returnValueInfo = $", Return value: {FormatValue(context.ReturnValue)}";
        }

        var logMessage = LogExecutionTime ? $"Method {methodName} executed successfully, Execution time: {executionTime.TotalMilliseconds:F2} ms{returnValueInfo}" : $"Method {methodName} executed successfully{returnValueInfo}";

        LoggingServiceImpl.InstanceVal.Log(LogLevel, logMessage);
    }

    /// <summary>
    /// Logs method execution failure with exception details and optional execution time information.
    /// </summary>
    /// <param name="methodName">The formatted name of the failed method.</param>
    /// <param name="executionTime">The time elapsed before method execution failed.</param>
    /// <param name="ex">The exception that caused the method execution to fail.</param>
    private void LogMethodFailure(string methodName, TimeSpan executionTime, Exception ex)
    {
        var logMessage = LogExecutionTime ? $"Method {methodName} execution failed, Execution time: {executionTime.TotalMilliseconds:F2} ms, Error message: {ex.Message}" : $"Method {methodName} execution failed, Error message: {ex.Message}";

        LoggingServiceImpl.InstanceVal.LogError(logMessage, ex);
    }

    /// <summary>
    /// Formats an object value into a string representation for logging purposes.
    /// </summary>
    /// <param name="value">The object value to format, can be null.</param>
    /// <returns>A formatted string representation of the input value.</returns>
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