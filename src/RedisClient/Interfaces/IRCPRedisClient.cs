using RedisClient.Models;
using System;
using System.Threading.Tasks;

namespace RedisClient.Interfaces
{
    public interface IRCPRedisClient
    {
        Task<Response<T>> GetValueAsync<T>(string key);
        Task SetValueAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<Response<T>> TryGetValueAsync<T>(string key);
    }
}