using GoofyAlgoTrader.Caching.Default;
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
    }
}
