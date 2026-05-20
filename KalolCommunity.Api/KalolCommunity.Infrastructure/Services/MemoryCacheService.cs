using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KalolCommunity.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace KalolCommunity.Infrastructure.Services
{
    public class MemoryCacheService : ICachingService
    {
        private readonly IMemoryCache _cache;

        public MemoryCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getData, TimeSpan expiry)
        {
            if (!_cache.TryGetValue(key, out T? data))
            {
                data = await getData();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };

                _cache.Set(key, data, cacheOptions);
            }
            return data!;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };

            _cache.Set(key, value, cacheOptions);
            await Task.CompletedTask;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return await Task.FromResult(value);
        }

        public async Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            await Task.CompletedTask;
        }
    }
}