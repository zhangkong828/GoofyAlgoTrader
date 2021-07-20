using CSRedis;
using GoofyAlgoTrader.Caching.Default;
using GoofyAlgoTrader.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Caching
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection UseDefaultCaching(this IServiceCollection services)
        {
            services.AddSingleton<ICachingProvider, MemoryCacheProvider>();

            return services;
        }

        public static IServiceCollection UseRedisCache(this IServiceCollection services, Action<RedisCacheConfig> configDelegate = null)
        {
            var config = Config.Get<RedisCacheConfig>("RedisCache") ?? new RedisCacheConfig();
            configDelegate?.Invoke(config);

            Check.NotNullOrEmpty(config.Connection, "地址");

            services.AddSingleton(config);

            var RedisClient = new CSRedisClient(config.Connection);
            services.AddSingleton(_ => RedisClient);

            services.AddSingleton<ICaching, RedisCache>();
            services.AddSingleton<ICachingProvider, RedisCacheProvider>();

            return services;
        }
    }
}
