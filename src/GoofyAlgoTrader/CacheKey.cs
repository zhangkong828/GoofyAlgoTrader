using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoofyAlgoTrader
{
    public class CacheKey
    {
        public static string WebApiToken(string userId)
        {
            return $"WebApiToken:{userId}";
        }


        public static string FuturesHashConfig()
        {
            return $"Futures:Config";
        }

        public static string FuturesInstrumentLastMin(string instrumentID)
        {
            return $"Futures:LastMin:{instrumentID}";
        }
    }
}
