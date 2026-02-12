using ArxOne.MrAdvice.Advice;
using LoggingService.Services;

namespace CommonFramework.Aop.Attributes;

/// <summary>
/// Retry advice that automatically retries failed method executions
/// Uses compile-time weaving for better performance
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RetryAttribute : Attribute, IMethodAdvice
{
    public int MaxRetries { get; set; } = 3;
    public int DelayMilliseconds { get; set; } = 1000;
    public Type[] ExceptionTypes { get; set; } = [typeof(Exception)];

    public void Advise(MethodAdviceContext context)
    {
        var methodName = $"{context.TargetType.Name}.{context.TargetMethod.Name}";
        LoggingServiceImpl.InstanceVal.LogDebug($"Starting retry mechanism for method: {methodName}");

        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    LoggingServiceImpl.InstanceVal.LogInformation($"Attempt {attempt + 1}/{MaxRetries + 1} for method: {methodName}");
                }

                context.Proceed(); // Execute the original method
                LoggingServiceImpl.InstanceVal.LogInformation($"Method {methodName} succeeded on attempt {attempt + 1}");
                return;
            }
            catch (Exception ex) when (ShouldRetry(ex))
            {
                if (attempt == MaxRetries)
                {
                    LoggingServiceImpl.InstanceVal.LogError($"Max retries ({MaxRetries + 1}) reached for method: {methodName}", ex);
                    throw;
                }

                var delay = DelayMilliseconds * (attempt + 1);
                LoggingServiceImpl.InstanceVal.LogWarning($"Exception occurred in method {methodName} on attempt {attempt + 1}: {ex.Message}. Retrying in {delay}ms...");
                Task.Delay(delay).Wait();
            }
        }
    }

    private bool ShouldRetry(Exception exception)
    {
        return ExceptionTypes.Any(exceptionType => exceptionType.IsInstanceOfType(exception));
    }
}