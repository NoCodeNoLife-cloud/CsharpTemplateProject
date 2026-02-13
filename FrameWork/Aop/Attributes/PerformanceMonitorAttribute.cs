using System.Diagnostics;
using ArxOne.MrAdvice.Advice;
using LoggingService.Services;

namespace CommonFramework.Aop.Attributes;

/// <summary>
/// Performance monitoring advice that tracks method execution time
/// Applied at compile-time for zero runtime impact
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PerformanceMonitorAttribute(int thresholdMilliseconds = 100) : Attribute, IMethodAdvice
{
    public TimeSpan Threshold { get; set; } = TimeSpan.FromMilliseconds(thresholdMilliseconds);

    public void Advise(MethodAdviceContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        context.Proceed();
        stopwatch.Stop();

        if (stopwatch.Elapsed <= Threshold) return;
        var methodName = $"{context.TargetType.Name}.{context.TargetMethod.Name}";
        LoggingServiceImpl.InstanceVal.LogWarning(
            $"Performance warning: Method {methodName} took {stopwatch.ElapsedMilliseconds}ms (threshold: {Threshold.TotalMilliseconds}ms)");
    }
}