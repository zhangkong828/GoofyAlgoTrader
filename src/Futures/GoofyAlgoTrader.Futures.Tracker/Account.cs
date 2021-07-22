using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class Account
    {
        public string ServerName { get; set; } = string.Empty;
        public string Investor { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Broker { get; set; } = string.Empty;
        public string AppID { get; set; } = string.Empty;
        public string AuthCode { get; set; } = string.Empty;
        public string ProductInfo { get; set; } = string.Empty;

        public string TradeFrontAddr { get; set; } = string.Empty;
        public string MarketFrontAddr { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{Investor}@{ServerName}";
        }
    }
}
