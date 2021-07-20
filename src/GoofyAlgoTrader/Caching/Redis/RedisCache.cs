using CSRedis;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Caching.Redis
{
    public class RedisCache : ICaching
    {
        private readonly CSRedisClient _client;
        public RedisCache(CSRedisClient client)
        {
            _client = client;
        }

        public bool Exists(string key)
        {
            return _client.Exists(key);
        }

        public string Get(string key)
        {
            return _client.Get(key);
        }

        public T Get<T>(string key)
        {
            return _client.Get<T>(key);
        }

        public bool Remove(string key)
        {
            return _client.Del(key) > 0;
        }

        public bool Set(string key, object data, TimeSpan? expiry = null)
        {
            var expireSeconds = -1;
            if (expiry.HasValue)
                expireSeconds = (int)expiry.Value.TotalSeconds;
            return _client.Set(key, data, expireSeconds);
        }
    }
}
