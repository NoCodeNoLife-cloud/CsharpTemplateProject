using FluentAssertions;
using LoggingService.Enums;
using LoggingService.Services;
using Xunit;

namespace Tests.LoggingTests;

public class LoggingServiceTests
{
    private readonly LoggingServiceImpl _loggingService = LoggingServiceImpl.InstanceVal;

    [Fact]
    public void Singleton_Instance_Should_Return_Same_Instance()
    {
        // Arrange & Act
        var instance1 = LoggingServiceImpl.InstanceVal;
        var instance2 = LoggingServiceImpl.InstanceVal;

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void LogInformation_Should_Not_Throw_Exception()
    {
        // Act
        Action act = () => _loggingService.LogInformation("Test information message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogWarning_Should_Not_Throw_Exception()
    {
        // Act
        Action act = () => _loggingService.LogWarning("Test warning message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogError_Without_Exception_Should_Not_Throw_Exception()
    {
        // Act
        Action act = () => _loggingService.LogError("Test error message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogError_With_Exception_Should_Not_Throw_Exception()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        Action act = () => _loggingService.LogError("Test error message", exception);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogDebug_Should_Not_Throw_Exception()
    {
        // Act
        Action act = () => _loggingService.LogDebug("Test debug message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogCritical_Without_Exception_Should_Not_Throw_Exception()
    {
        // Act
        Action act = () => _loggingService.LogCritical("Test critical message");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LogCritical_With_Exception_Should_Not_Throw_Exception()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        Action act = () => _loggingService.LogCritical("Test critical message", exception);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Critical)]
    public void Log_With_Different_Levels_Should_Not_Throw_Exception(LogLevel level)
    {
        // Act
        Action act = () => _loggingService.Log(level, $"Test message for {level}");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Log_With_Exception_Should_Not_Throw_Exception()
    {
        // Arrange
        var exception = new ArgumentException("Test argument exception");

        // Act
        Action act = () => _loggingService.Log(LogLevel.Error, "Test message with exception", exception);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Multiple_Log_Calls_Should_Not_Interfere_With_Each_Other()
    {
        // Act & Assert
        Action act1 = () => _loggingService.LogInformation("First message");
        Action act2 = () => _loggingService.LogWarning("Second message");
        Action act3 = () => _loggingService.LogError("Third message");

        act1.Should().NotThrow();
        act2.Should().NotThrow();
        act3.Should().NotThrow();
    }
}