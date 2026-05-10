using System.Collections.Concurrent;

namespace wangluoanqan.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var bucket = _buckets.GetOrAdd(clientIp, _ => new TokenBucket(100, 10));
        if (bucket.TryConsume(1))
        {
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Too many requests. Please slow down.");
        }
    }

    private class TokenBucket
    {
        private double _tokens;
        private readonly int _capacity;
        private readonly double _refillRate;
        private DateTime _lastRefill;

        public TokenBucket(int capacity, double refillRate)
        {
            _capacity = capacity;
            _refillRate = refillRate;
            _tokens = capacity;
            _lastRefill = DateTime.UtcNow;
        }

        public bool TryConsume(int tokens)
        {
            Refill();
            if (_tokens >= tokens)
            {
                _tokens -= tokens;
                return true;
            }
            return false;
        }

        private void Refill()
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - _lastRefill).TotalSeconds;
            _tokens = Math.Min(_capacity, _tokens + elapsed * _refillRate);
            _lastRefill = now;
        }
    }
}