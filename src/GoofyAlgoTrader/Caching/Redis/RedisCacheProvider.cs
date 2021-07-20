using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Caching.Redis
{
    public class RedisCacheProvider : ICachingProvider
    {
        private readonly ICaching _caching;
        public RedisCacheProvider(ICaching caching)
        {
            _caching = caching;
        }

        public ICaching CreateCaching()
        {
            return _caching;
        }
    }
}
