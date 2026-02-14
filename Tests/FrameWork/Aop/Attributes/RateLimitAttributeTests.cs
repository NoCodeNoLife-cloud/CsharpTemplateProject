using System.Reflection;
using CommonFramework.Aop.Attributes;

namespace Tests.FrameWork.Aop.Attributes;

public class RateLimitAttributeTests
{
    [Fact]
    public void RateLimitAttribute_ShouldHaveDefaultValues()
    {
        // Arrange
        var attribute = new RateLimitAttribute();

        // Assert
        attribute.MaxRequests.Should().Be(100);
        attribute.Period.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void RateLimitAttribute_ShouldAllowCustomConfiguration()
    {
        // Arrange
        var attribute = new RateLimitAttribute
        {
            MaxRequests = 50,
            Period = TimeSpan.FromSeconds(30)
        };

        // Assert
        attribute.MaxRequests.Should().Be(50);
        attribute.Period.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void TokenBucket_ShouldInitializeWithFullTokens()
    {
        // Arrange
        var bucket = CreateTokenBucket(10, TimeSpan.FromMinutes(1));

        // Act
        var canAcquire = InvokeTryAcquirePermit(bucket);

        // Assert
        canAcquire.Should().BeTrue();
    }

    [Fact]
    public void TokenBucket_ShouldDepleteTokens()
    {
        // Arrange
        var bucket = CreateTokenBucket(3, TimeSpan.FromMinutes(1));
        var acquiredTokens = new List<bool>();

        // Act - Acquire all tokens
        for (var i = 0; i < 5; i++)
        {
            acquiredTokens.Add(InvokeTryAcquirePermit(bucket));
        }

        // Assert
        acquiredTokens.Take(3).Should().AllBeEquivalentTo(true); // First 3 should succeed
        acquiredTokens.Skip(3).Should().AllBeEquivalentTo(false); // Remaining should fail
    }

    [Fact]
    public void TokenBucket_ShouldRefillOverTime()
    {
        // Arrange
        var shortPeriod = TimeSpan.FromMilliseconds(100);
        var bucket = CreateTokenBucket(5, shortPeriod);

        // Deplete all tokens
        for (var i = 0; i < 5; i++)
        {
            InvokeTryAcquirePermit(bucket).Should().BeTrue();
        }

        // Verify no more tokens available
        InvokeTryAcquirePermit(bucket).Should().BeFalse();

        // Act - Wait for refill
        Thread.Sleep(shortPeriod + TimeSpan.FromMilliseconds(50));

        // Assert - Should be able to acquire tokens again
        InvokeTryAcquirePermit(bucket).Should().BeTrue();
    }

    [Fact]
    public void TokenBucket_ShouldHandlePartialRefill()
    {
        // Arrange
        var period = TimeSpan.FromMilliseconds(200);
        var bucket = CreateTokenBucket(10, period);

        // Use most of the tokens
        for (int i = 0; i < 8; i++) // Use 8 out of 10 tokens
        {
            InvokeTryAcquirePermit(bucket).Should().BeTrue();
        }

        // Wait for small partial refill (1/10th of the period)
        Thread.Sleep(period.Divide(10));

        // Act & Assert - Should have some tokens refilled
        var successCount = 0;
        for (int i = 0; i < 4; i++) // Try to acquire 4 times
        {
            if (InvokeTryAcquirePermit(bucket))
                successCount++;
        }

        successCount.Should().BeGreaterThan(0); // At least some should succeed
        successCount.Should().BeLessThan(4); // But not all should succeed
    }

    [Fact]
    public void IntegrationTest_RateLimitAttributeShouldEnforceLimits()
    {
        // Arrange
        var testService = new TestRateLimitService();

        // Act & Assert - First calls should succeed
        for (var i = 0; i < 3; i++)
        {
            var result = testService.LimitedMethod();
            Assert.Equal("Success", result);
        }

        // Next call should fail due to rate limit
        var exception = Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => testService.LimitedMethod());
        Assert.Contains("Rate limit exceeded", exception.Message);
    }

    [Fact]
    public void IntegrationTest_DifferentMethodsShouldHaveSeparateRateLimits()
    {
        // Act - Call first method until limit reached
        for (var i = 0; i < 2; i++)
        {
            TestRateLimitService.FirstLimitedMethod();
        }

        // Assert - Second method should still work
        var result = TestRateLimitService.SecondLimitedMethod();
        Assert.Equal("Second method success", result);

        // But first method should now be rate limited
        Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => TestRateLimitService.FirstLimitedMethod());
    }

    [Fact]
    public void IntegrationTest_RateLimitShouldResetOverTime()
    {
        // Arrange

        // Exhaust the rate limit
        for (var i = 0; i < 2; i++)
        {
            TestSlowRefillService.SlowMethod();
        }

        // Verify rate limit is hit
        Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => TestSlowRefillService.SlowMethod());

        // Wait for refill (250ms > 200ms period)
        Thread.Sleep(250);

        // Act - Should be able to call again
        var result = TestSlowRefillService.SlowMethod();

        // Assert
        Assert.Equal("Success after wait", result);
    }

    [Fact]
    public async Task TokenBucket_ShouldBeThreadSafe()
    {
        // Arrange
        var bucket = CreateTokenBucket(100, TimeSpan.FromMinutes(1));
        var concurrentTasks = new List<Task<int>>(); // Changed to Task<int> to track actual successes
        const int threadCount = 10;
        const int attemptsPerThread = 15; // Total 150 attempts for 100 tokens

        // Act - Run concurrent acquisition attempts
        for (var i = 0; i < threadCount; i++)
        {
            concurrentTasks.Add(Task.Run(() =>
            {
                var successes = 0;
                for (var j = 0; j < attemptsPerThread; j++)
                {
                    if (InvokeTryAcquirePermit(bucket))
                        successes++;
                }

                return successes; // Return actual count instead of boolean
            }));
        }

        var results = await Task.WhenAll(concurrentTasks);

        // Assert - Most threads should have had some success (relaxed assertion)
        var successfulThreads = results.Count(r => r > 0);
        successfulThreads.Should().BeGreaterThan(threadCount / 2); // At least half should succeed

        // Verify that we didn't exceed the token limit
        var totalSuccesses = results.Sum();
        totalSuccesses.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void RateLimitAttribute_ShouldWorkWithClassLevelAttribute()
    {
        // Arrange

        // Act & Assert - First 5 calls should succeed for each method
        for (var i = 0; i < 5; i++)
        {
            var result = ClassLevelRateLimitService.MethodOne();
            Assert.Equal("Method One", result);

            result = ClassLevelRateLimitService.MethodTwo();
            Assert.Equal("Method Two", result);
        }

        // Both methods should now be rate limited (6th call and beyond)
        Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => ClassLevelRateLimitService.MethodOne());
        Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => ClassLevelRateLimitService.MethodTwo());

        // Verify that subsequent calls also fail
        Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => ClassLevelRateLimitService.MethodOne());
        Assert.Throws<RateLimitAttribute.RateLimitExceededException>(() => ClassLevelRateLimitService.MethodTwo());
    }

    private static object CreateTokenBucket(int capacity, TimeSpan period)
    {
        var ctor = typeof(RateLimitAttribute).GetNestedType("TokenBucket", BindingFlags.NonPublic)
            ?.GetConstructor([typeof(int), typeof(TimeSpan)]);

        return ctor == null ? throw new InvalidOperationException("TokenBucket constructor not found") : ctor.Invoke([capacity, period]);
    }

    private static bool InvokeTryAcquirePermit(object bucket)
    {
        var method = bucket.GetType().GetMethod("TryAcquirePermit", BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
        {
            throw new InvalidOperationException("TryAcquirePermit method not found");
        }

        var result = method.Invoke(bucket, []);
        return result as bool? ?? throw new InvalidOperationException("TryAcquirePermit method returned unexpected type");
    }
}

// Test services with rate limit attributes
[RateLimit(MaxRequests = 3, PeriodMilliseconds = 60000)]
public class TestRateLimitService
{
    public string TestMethod() => "Test";

    public string LimitedMethod()
    {
        return "Success";
    }

    [RateLimit(MaxRequests = 2, PeriodMilliseconds = 60000)]
    public static string FirstLimitedMethod()
    {
        return "First method success";
    }

    [RateLimit(MaxRequests = 5, PeriodMilliseconds = 60000)]
    public static string SecondLimitedMethod()
    {
        return "Second method success";
    }
}

[RateLimit(MaxRequests = 2, PeriodMilliseconds = 200)]
public class TestSlowRefillService
{
    public static string SlowMethod()
    {
        return "Success after wait";
    }
}

[RateLimit(MaxRequests = 5, PeriodMilliseconds = 60000)]
public class ClassLevelRateLimitService
{
    public static string MethodOne()
    {
        return "Method One";
    }

    public static string MethodTwo()
    {
        return "Method Two";
    }
}