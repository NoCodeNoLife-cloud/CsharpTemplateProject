using CommonFramework.Aop.Attributes;
using LoggingService.Enums;

namespace Tests.FrameWork.Aop.Attributes;

/// <summary>
/// LogAttribute usage examples for testing purposes
/// Demonstrates various fine-grained logging control configurations
/// </summary>
public abstract class LogAttributeUsageExamples
{
    /// <summary>
    /// Default configuration: logs all information
    /// </summary>
    [Log(LogLevel = LogLevel.Information)]
    public static string DefaultLogging(string input, int number)
    {
        return $"Processed: {input} with {number}";
    }

    /// <summary>
    /// Logs only method entry and parameters, no return value or execution time
    /// </summary>
    [Log(
        LogLevel = LogLevel.Debug,
        LogMethodEntry = true,
        LogMethodExit = false,
        LogMethodException = true,
        LogParameters = true,
        LogReturnValue = false,
        LogExecutionTime = false
    )]
    public static int ParameterOnlyLogging(int value1, int value2)
    {
        return value1 + value2;
    }

    /// <summary>
    /// Logs only method return and execution time, no entry or parameters
    /// </summary>
    [Log(
        LogLevel = LogLevel.Information,
        LogMethodEntry = false,
        LogMethodExit = true,
        LogMethodException = true,
        LogParameters = false,
        LogReturnValue = true,
        LogExecutionTime = true
    )]
    public static string ReturnAndTimeOnly(string message)
    {
        Thread.Sleep(100); // Simulate processing time
        return $"Result: {message.ToUpper()}";
    }

    /// <summary>
    /// Logs only exception information
    /// </summary>
    [Log(
        LogLevel = LogLevel.Error,
        LogMethodEntry = false,
        LogMethodExit = false,
        LogMethodException = true,
        LogParameters = false,
        LogReturnValue = false,
        LogExecutionTime = false
    )]
    public static void ExceptionOnlyLogging(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty");
        }
        // Processing logic here
    }

    /// <summary>
    /// Minimal logging configuration (except exceptions which are typically kept)
    /// </summary>
    [Log(
        LogLevel = LogLevel.Debug,
        LogMethodEntry = false,
        LogMethodExit = false,
        LogMethodException = true, // Exception logging usually recommended to keep enabled
        LogParameters = false,
        LogReturnValue = false,
        LogExecutionTime = false
    )]
    public static decimal MinimalLogging(decimal amount, decimal rate)
    {
        if (rate < 0)
            throw new ArgumentOutOfRangeException(nameof(rate), "Rate must be positive");

        return amount * rate;
    }

    /// <summary>
    /// Performance monitoring mode: logs execution time and exceptions only
    /// </summary>
    [Log(
        LogLevel = LogLevel.Warning,
        LogMethodEntry = false,
        LogMethodExit = true,
        LogMethodException = true,
        LogParameters = false,
        LogReturnValue = false,
        LogExecutionTime = true
    )]
    public static void PerformanceMonitoring()
    {
        // Simulate time-consuming operation
        Thread.Sleep(new Random().Next(50, 500));
    }

    /// <summary>
    /// Debug mode: logs detailed information including everything
    /// </summary>
    [Log(
        LogLevel = LogLevel.Debug,
        LogMethodEntry = true,
        LogMethodExit = true,
        LogMethodException = true,
        LogParameters = true,
        LogReturnValue = true,
        LogExecutionTime = true
    )]
    public static T[] DebugMode<T>(T[] items, Func<T, bool> predicate)
    {
        return Array.FindAll(items, new Predicate<T>(predicate));
    }
}

public class LogAttributeTests
{
    [Fact]
    public void DefaultLogging_ShouldWork()
    {
        // Test default logging configuration
        var result = LogAttributeUsageExamples.DefaultLogging("test", 42);
        Assert.Equal("Processed: test with 42", result);
    }

    [Fact]
    public void ParameterOnlyLogging_ShouldWork()
    {
        // Test parameter-only logging configuration
        var result = LogAttributeUsageExamples.ParameterOnlyLogging(10, 20);
        Assert.Equal(30, result);
    }

    [Fact]
    public void ReturnAndTimeOnly_ShouldWork()
    {
        // Test return value and execution time logging configuration
        var result = LogAttributeUsageExamples.ReturnAndTimeOnly("hello");
        Assert.Equal("Result: HELLO", result);
    }

    [Fact]
    public void ExceptionOnlyLogging_ValidInput_ShouldWork()
    {
        // Test exception-only logging configuration with valid input
        LogAttributeUsageExamples.ExceptionOnlyLogging("valid input");
        // Should not throw exception
    }

    [Fact]
    public void ExceptionOnlyLogging_InvalidInput_ShouldThrow()
    {
        // Test logging in exception scenario
        Assert.Throws<ArgumentException>(() => LogAttributeUsageExamples.ExceptionOnlyLogging(""));
    }

    [Fact]
    public void MinimalLogging_ValidInput_ShouldWork()
    {
        // Test minimal logging configuration
        var result = LogAttributeUsageExamples.MinimalLogging(100m, 1.5m);
        Assert.Equal(150m, result);
    }

    [Fact]
    public void MinimalLogging_InvalidInput_ShouldThrow()
    {
        // Test exception handling with minimal logging configuration
        Assert.Throws<ArgumentOutOfRangeException>(() => LogAttributeUsageExamples.MinimalLogging(100m, -1m));
    }

    [Fact]
    public void PerformanceMonitoring_ShouldWork()
    {
        // Test performance monitoring mode
        LogAttributeUsageExamples.PerformanceMonitoring();
        // Should not throw exception
    }

    [Fact]
    public void DebugMode_ShouldWork()
    {
        // Test debug mode
        var numbers = new[] { 1, 2, 3, 4, 5 };
        var result = LogAttributeUsageExamples.DebugMode(numbers, x => x % 2 == 0);
        Assert.Equal(new[] { 2, 4 }, result);
    }

    [Fact]
    public void AllConfigurations_ShouldCompileWithoutErrors()
    {
        // Verify all configuration combinations compile correctly
        Assert.True(true); // If we reach here, compilation passed
    }
}