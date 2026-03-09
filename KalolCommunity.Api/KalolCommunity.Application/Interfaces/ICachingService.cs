using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface ICachingService
    {
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getData, TimeSpan expiry);
    }
}
