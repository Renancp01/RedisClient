using Microsoft.Extensions.Options;
using RedisClient.Interfaces;
using RedisClient.Models;
using StackExchange.Redis;
using System;
using System.Text.Json;

namespace RedisClient.Services
{
    public class RCPRedisClient : IRCPRedisClient, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RCPRedisClient(IOptions<RCPRedisOptions> options)
        {
            _redis = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
            _database = _redis.GetDatabase();
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }

        public T GetValue<T>(string key)
        {
            var value = _database.StringGet(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        public void SetValue<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value);

            _database.StringSet(key, json);
        }
    }
}
