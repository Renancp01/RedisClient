using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RedisClient.Interfaces;
using RedisClient.Models;
using StackExchange.Redis;

namespace RedisClient.Services
{
    public class RcpRedisClient : IRCPRedisClient, IDisposable
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RcpRedisClient(IOptions<RCPRedisOptions> options)
        {
            _redis = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
            _database = _redis.GetDatabase();
        }

        public async Task<Response<T>> GetValueAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                return await HandleDeserialization<T>(key, value);
            }
            catch (Exception ex)
            {
                return new Response<T> { IsValid = false, Errors = new List<string> { ex.Message } };
            }
        }

        public async Task SetValueAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, json, expiry);
        }

        public async Task<Response<T>> TryGetValueAsync<T>(string key)
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                return await HandleDeserialization<T>(key, value);
            }
            catch (Exception ex)
            {
                return new Response<T> { IsValid = false, Errors = new List<string> { ex.Message } };
            }
        }

        private async Task<Response<T>> HandleDeserialization<T>(string key, RedisValue value)
        {
            if (value.IsNullOrEmpty || value.IsNull)
                return new Response<T> { IsValid = false, Errors = new List<string> { "Key not found" } };

            if (value.ToString() == "null")
                await _database.KeyDeleteAsync(key);

            try
            {
                var data = JsonSerializer.Deserialize<T>(value.ToString());
                return new Response<T> { IsValid = true, Data = data };
            }
            catch (JsonException)
            {
                return new Response<T> { IsValid = false, Errors = new List<string> { "Invalid data type" } };
            }
        }

        public void Dispose()
        {
            _redis.Dispose();
        }
    }
}
