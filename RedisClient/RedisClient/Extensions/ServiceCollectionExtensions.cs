using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisClient.Interfaces;
using RedisClient.Models;
using RedisClient.Services;

namespace RedisClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RCPRedisOptions>(configuration.GetSection("Redis"));
            services.AddSingleton<IRCPRedisClient, RCPRedisClient>();
            return services;
        }
    }
}
