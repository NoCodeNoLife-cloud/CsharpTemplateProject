using AspectCore.DynamicProxy;
using LoggingService.Enums;
using LoggingService.Interfaces;
using LoggingService.Services;
using System.Runtime.CompilerServices;

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
    private const string UnknownType = "Unknown";
    private readonly ILoggingService _loggingService;

    /// <summary>
    /// Initializes a new instance of the LogAttribute class.
    /// </summary>
    public LogAttribute()
    {
        _loggingService = LoggingServiceImpl.InstanceVal;
        LogLevel = LogLevel.Debug;
    }

    /// <summary>
    /// Initializes a new instance of the LogAttribute class with custom logging service.
    /// </summary>
    /// <param name="loggingService">The logging service to use.</param>
    public LogAttribute(ILoggingService loggingService)
    {
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        LogLevel = LogLevel.Debug;
    }

    /// <summary>
    /// Gets or sets the log level for logging method execution events.
    /// </summary>
    /// <value>The log level; defaults to LogLevel.Debug.</value>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log method parameters.
    /// </summary>
    /// <value>True to log method parameters; otherwise false. Defaults to false.</value>
    public bool LogParameters { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log method return values.
    /// </summary>
    /// <value>True to log method return values; otherwise false. Defaults to false.</value>
    public bool LogReturnValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log method execution time.
    /// </summary>
    /// <value>True to log execution time; otherwise false. Defaults to false.</value>
    public bool LogExecutionTime { get; set; }

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
        if (context.ServiceMethod == null)
            throw new ArgumentNullException(nameof(context));

        var methodInfo = context.ServiceMethod;
        var methodName = GetMethodName(methodInfo);
        var startTime = DateTimeOffset.Now;

        LogMethodEntry(methodName, methodInfo, context);

        try
        {
            await next(context);
            LogMethodExit(methodName, startTime, context, null);
        }
        catch (Exception ex)
        {
            LogMethodExit(methodName, startTime, context, ex);
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
        var declaringTypeName = methodInfo.DeclaringType?.Name ?? UnknownType;
        var methodName = methodInfo.Name;
        return $"{declaringTypeName}.{methodName}";
    }

    /// <summary>
    /// Logs method entry with optional parameter information.
    /// </summary>
    private void LogMethodEntry(string methodName, System.Reflection.MethodInfo methodInfo, AspectContext context,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var logMessage = BuildLogMessage(methodName, filePath, lineNumber, true);

        if (LogParameters && context.Parameters?.Length > 0)
        {
            var paramInfo = BuildParameterInfo(methodInfo, context.Parameters);
            logMessage += $", Parameters: {paramInfo}";
        }

        _loggingService.Log(LogLevel, logMessage);
    }

    /// <summary>
    /// Logs method exit with success or failure information.
    /// </summary>
    private void LogMethodExit(string methodName, DateTimeOffset startTime, AspectContext context, Exception? exception,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var executionTime = DateTimeOffset.Now - startTime;
        var isSuccess = exception == null;

        var baseMessage = BuildLogMessage(methodName, filePath, lineNumber, isSuccess);
        var timeInfo = LogExecutionTime ? $", Execution time: {executionTime.TotalMilliseconds:F2} ms" : string.Empty;
        var returnValueInfo = BuildReturnValueInfo(context);
        var errorMessage = exception != null ? $", Error message: {exception.Message}" : string.Empty;

        var logMessage = $"{baseMessage}{timeInfo}{returnValueInfo}{errorMessage}";

        if (isSuccess)
        {
            _loggingService.Log(LogLevel, logMessage);
        }
        else
        {
            _loggingService.LogError(logMessage, exception!);
        }
    }

    /// <summary>
    /// Builds the base log message structure.
    /// </summary>
    private string BuildLogMessage(string methodName, string filePath, int lineNumber, bool isSuccess)
    {
        var status = isSuccess ? "executed successfully" : "execution failed";
        return $"Method {methodName} {status} at {filePath}:{lineNumber}";
    }

    /// <summary>
    /// Builds return value information for logging.
    /// </summary>
    private string BuildReturnValueInfo(AspectContext context)
    {
        if (!LogReturnValue || context.ReturnValue == null)
            return string.Empty;

        return $", Return value: {FormatValue(context.ReturnValue)}";
    }

    /// <summary>
    /// Builds a formatted string representation of method parameters and their values.
    /// </summary>
    /// <param name="methodInfo">The method information to extract parameter names.</param>
    /// <param name="parameters">The array of parameter values passed to the method.</param>
    /// <returns>A formatted string containing parameter names and their values.</returns>
    private static string BuildParameterInfo(System.Reflection.MethodInfo methodInfo, object[] parameters)
    {
        if (parameters.Length == 0)
            return string.Empty;

        var methodParameters = methodInfo.GetParameters();
        var paramStrings = new List<string>();

        for (int i = 0; i < parameters.Length && i < methodParameters.Length; i++)
        {
            var paramName = methodParameters[i].Name ?? "unknown";
            var paramValue = FormatValue(parameters[i]);
            paramStrings.Add($"{paramName}={paramValue}");
        }

        return string.Join(", ", paramStrings);
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