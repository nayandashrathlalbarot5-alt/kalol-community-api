using System;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces.Infrastructure
{
    public interface ICachingService
    {
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getData, TimeSpan expiry);
        Task SetAsync<T>(string key, T value, TimeSpan expiry);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
