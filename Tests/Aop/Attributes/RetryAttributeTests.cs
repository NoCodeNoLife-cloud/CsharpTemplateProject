using CommonFramework.Aop.Attributes;
using System.Reflection;

namespace Tests.Aop;

public class RetryAttributeTests
{
    [Fact]
    public void RetryAttribute_ShouldHaveDefaultValues()
    {
        // Arrange
        var attribute = new RetryAttribute();

        // Assert
        attribute.MaxRetries.Should().Be(3);
        attribute.DelayMilliseconds.Should().Be(1000);
        attribute.ExceptionTypes.Should().ContainSingle().Which.Should().Be<Exception>();
    }

    [Fact]
    public void RetryAttribute_ShouldAllowCustomConfiguration()
    {
        // Arrange
        var attribute = new RetryAttribute
        {
            MaxRetries = 5,
            DelayMilliseconds = 2000,
            ExceptionTypes = [typeof(InvalidOperationException), typeof(ArgumentException)]
        };

        // Assert
        attribute.MaxRetries.Should().Be(5);
        attribute.DelayMilliseconds.Should().Be(2000);
        attribute.ExceptionTypes.Should().HaveCount(2);
        attribute.ExceptionTypes.Should().Contain(typeof(InvalidOperationException));
        attribute.ExceptionTypes.Should().Contain(typeof(ArgumentException));
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrueForMatchingExceptionTypes()
    {
        // Arrange
        var attribute = new RetryAttribute
        {
            ExceptionTypes = [typeof(ArgumentException), typeof(InvalidOperationException)]
        };

        // Act & Assert
        var result1 = InvokeShouldRetry(attribute, new ArgumentException());
        var result2 = InvokeShouldRetry(attribute, new InvalidOperationException());

        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalseForNonMatchingExceptionTypes()
    {
        // Arrange
        var attribute = new RetryAttribute
        {
            ExceptionTypes = [typeof(ArgumentNullException)]
        };

        // Act & Assert
        var result = InvokeShouldRetry(attribute, new InvalidOperationException());
        result.Should().BeFalse();
    }

    [Fact]
    public void IntegrationTest_RetryAttributeShouldWorkWithActualMethods()
    {
        // Arrange
        var testService = new TestRetryService();

        // Act
        var result = testService.MethodThatMightFail();

        // Assert
        result.Should().Be("Success after retry");
        testService.CallCount.Should().Be(2); // Initial call + 1 retry
    }

    [Fact]
    public void IntegrationTest_RetryAttributeShouldRespectMaxRetries()
    {
        // Arrange
        var testService = new TestRetryService();

        // Act
        var action = () => testService.MethodThatAlwaysFails();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Always fails");
        testService.CallCount.Should().Be(4); // Initial + 3 retries (MaxRetries = 3)
    }

    [Fact]
    public void IntegrationTest_RetryAttributeShouldOnlyRetrySpecificExceptions()
    {
        // Arrange
        var testService = new TestSelectiveRetryService();

        // Act
        var action = () => testService.MethodWithDifferentExceptionTypes();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("This should not be retried");
        testService.RetryCallCount.Should().Be(2); // Retried once for ArgumentException, then failed on InvalidOperationException
    }

    private static bool InvokeShouldRetry(RetryAttribute attribute, Exception exception)
    {
        // Use reflection to access private ShouldRetry method
        var method = typeof(RetryAttribute).GetMethod("ShouldRetry", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            throw new InvalidOperationException("ShouldRetry method not found");
        }

        var result = method.Invoke(attribute, [exception]);
        return result as bool? ?? throw new InvalidOperationException("ShouldRetry method returned unexpected type");
    }
}

// Test service classes that use the RetryAttribute
[Retry(MaxRetries = 3, DelayMilliseconds = 10)]
public class TestRetryService
{
    public int CallCount { get; private set; }

    [Retry(MaxRetries = 1, DelayMilliseconds = 1)] // Override with fewer retries for testing
    public string MethodThatMightFail()
    {
        CallCount++;
        return CallCount == 1 ? throw new InvalidOperationException("First attempt fails") : "Success after retry";
    }

    public string MethodThatAlwaysFails()
    {
        CallCount++;
        throw new InvalidOperationException("Always fails");
    }
}

[Retry(MaxRetries = 2, ExceptionTypes = [typeof(ArgumentException)])]
public class TestSelectiveRetryService
{
    public int RetryCallCount { get; private set; }
    public int NonRetryCallCount { get; private set; }

    public string MethodThatThrowsRetryableException()
    {
        RetryCallCount++;
        return RetryCallCount == 1 ? throw new ArgumentException("This should be retried") : "Success after retry";
    }

    public string MethodThatThrowsNonRetryableException()
    {
        NonRetryCallCount++;
        throw new InvalidOperationException("Non-retryable exception");
    }

    public string MethodWithDifferentExceptionTypes()
    {
        RetryCallCount++;
        return RetryCallCount switch
        {
            1 => throw new ArgumentException("This should be retried"),
            2 => throw new InvalidOperationException("This should not be retried"),
            _ => "Success"
        };
    }
}