﻿using GoofyAlgoTrader.Caching;
using GoofyAlgoTrader.Caching.Redis;
using GoofyAlgoTrader.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Configurations
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder AddConfigFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            builder.AddJsonFile(path, optional: optional, reloadOnChange: reloadOnChange);

            var configurationBuilder = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
             .AddJsonFile(path, optional: optional, reloadOnChange: reloadOnChange);

            Config.Configuration = configurationBuilder.Build();
            Config.Options = Config.Get<ConfigOptions>("GoofyAlgoTrader") ?? new ConfigOptions();
            Config.LoggerOptions = Config.Get<LoggerOptions>("GoofyAlgoTrader:Logger") ?? new LoggerOptions();
            return builder;
        }

        public static IServiceCollection UseGoofyAlgoTrader(this IServiceCollection services)
        {
            var redisCache = Config.Get<RedisCacheConfig>("RedisCache");
            if (redisCache == null)
                services.UseDefaultCaching();
            else
                services.UseRedisCache();

            return services;
        }
    }
}
