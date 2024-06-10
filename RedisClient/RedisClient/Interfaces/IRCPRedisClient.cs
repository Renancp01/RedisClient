namespace RedisClient.Interfaces
{
    public interface IRCPRedisClient
    {
        T GetValue<T>(string key);
        
        void SetValue<T>(string key, T value);
        
        bool TryGetValue<T>(string key, out T value);
    }
}