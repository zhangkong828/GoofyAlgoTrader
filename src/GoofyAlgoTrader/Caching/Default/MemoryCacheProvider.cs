using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Caching.Default
{
    public class MemoryCacheProvider : ICachingProvider
    {
        private readonly MemoryCache _memoryCache;
        public MemoryCacheProvider()
        {
            _memoryCache = new MemoryCache();
        }

        public ICaching CreateCaching()
        {
            return _memoryCache;
        }
    }
}
