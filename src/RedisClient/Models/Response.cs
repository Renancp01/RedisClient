using System.Collections.Generic;

namespace RedisClient.Models
{
    public class Response<T>
    {
        public bool IsValid { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}