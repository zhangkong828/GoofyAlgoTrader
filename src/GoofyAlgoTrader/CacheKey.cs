using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public class CacheKey
    {
        public static readonly string PrefixKey = "GoofyAlgoTrader:";

        public static string WebApiToken(string userId)
        {
            return $"{PrefixKey}WebApiToken:{userId}";
        }

        public static string FuturesHashConfig()
        {
            return $"{PrefixKey}Futures:Config";
        }

        public static string FuturesInstrumentLastMin()
        {
            return $"{PrefixKey}Futures:LastMin";
        }
    }
}
