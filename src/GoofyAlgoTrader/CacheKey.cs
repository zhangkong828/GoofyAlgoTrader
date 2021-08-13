﻿using System;
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
            return $"GoofyAlgoTrader:WebApiToken:{userId}";
        }

        public static string Futures()
        {
            return $"GoofyAlgoTrader:Futures";
        }

        public static string FuturesHashConfig()
        {
            return $"GoofyAlgoTrader:Futures:Config";
        }

        public static string FuturesInstrumentLastMin(string instrumentID)
        {
            return $"GoofyAlgoTrader:Futures:LastMin:{instrumentID}";
        }
    }
}
