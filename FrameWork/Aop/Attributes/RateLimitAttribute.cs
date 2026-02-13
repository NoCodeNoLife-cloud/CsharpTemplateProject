using System.Collections.Concurrent;
using ArxOne.MrAdvice.Advice;

namespace CommonFramework.Aop.Attributes;

/// <summary>
/// Rate limiting advice using token bucket algorithm
/// Applied at compile-time for zero runtime overhead
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RateLimitAttribute : Attribute, IMethodAdvice
{
    private static readonly ConcurrentDictionary<string, TokenBucket> TokenBuckets = new();
    public int MaxRequests { get; set; } = 100;
    public int PeriodMilliseconds { get; set; } = 60000; // 1 minute in milliseconds

    // Convenience property to work with TimeSpan internally
    public TimeSpan Period
    {
        get => TimeSpan.FromMilliseconds(PeriodMilliseconds);
        set => PeriodMilliseconds = (int)value.TotalMilliseconds;
    }

    public void Advise(MethodAdviceContext context)
    {
        var key = GetRateLimitKey(context);
        var bucket = TokenBuckets.GetOrAdd(key, _ => new TokenBucket(MaxRequests, TimeSpan.FromMilliseconds(PeriodMilliseconds)));

        if (!bucket.TryAcquirePermit())
        {
            throw new RateLimitExceededException($"Rate limit exceeded for {key}");
        }

        context.Proceed(); // Execute the original method
    }

    private static string GetRateLimitKey(MethodAdviceContext context)
    {
        return $"{context.TargetType.FullName}.{context.TargetMethod.Name}";
    }

    public class RateLimitExceededException : Exception
    {
        public RateLimitExceededException(string message) : base(message)
        {
        }

        public RateLimitExceededException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    private class TokenBucket(int capacity, TimeSpan period)
    {
        private readonly int _capacity = capacity;
        private readonly Lock _lock = new();
        private int _tokens = capacity;
        private DateTime _lastRefill = DateTime.UtcNow;

        public bool TryAcquirePermit()
        {
            lock (_lock)
            {
                RefillTokens();
                if (_tokens <= 0) return false;
                _tokens--;
                return true;
            }
        }

        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRefill;
            var tokensToAdd = (int)(elapsed.TotalMilliseconds / period.TotalMilliseconds * _capacity);

            if (tokensToAdd <= 0) return;
            _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
            _lastRefill = now;
        }
    }
}