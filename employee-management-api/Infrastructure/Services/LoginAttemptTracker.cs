using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services
{
    public class LoginAttemptTracker
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private readonly IMemoryCache _cache;

        public LoginAttemptTracker(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool IsLockedOut(string ipAddress, out TimeSpan remaining)
        {
            if (_cache.TryGetValue<DateTime>(LockoutKey(ipAddress), out var lockedUntil) && lockedUntil > DateTime.UtcNow)
            {
                remaining = lockedUntil - DateTime.UtcNow;
                return true;
            }

            remaining = TimeSpan.Zero;
            return false;
        }

        public void RecordFailure(string ipAddress)
        {
            var attemptsKey = AttemptsKey(ipAddress);
            var attempts = _cache.TryGetValue<int>(attemptsKey, out var existing) ? existing : 0;
            attempts++;

            if (attempts >= MaxAttempts)
            {
                _cache.Set(LockoutKey(ipAddress), DateTime.UtcNow.Add(LockoutDuration), LockoutDuration);
                _cache.Remove(attemptsKey);
            }
            else
            {
                _cache.Set(attemptsKey, attempts, LockoutDuration);
            }
        }

        public void RecordSuccess(string ipAddress)
        {
            _cache.Remove(AttemptsKey(ipAddress));
            _cache.Remove(LockoutKey(ipAddress));
        }

        private static string AttemptsKey(string ipAddress) => $"login_attempts_{ipAddress}";
        private static string LockoutKey(string ipAddress) => $"login_lockout_{ipAddress}";
    }
}