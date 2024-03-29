﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class Bar
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 交易日
        /// </summary>
        public string TradingDay { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        public double Open { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        public double High { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        public double Low { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public double Close { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        public int Volume { get; set; }

        /// <summary>
        /// 持仓量
        /// </summary>
        public double OpenInterest { get; set; }

        /// <summary>
        /// 前Bar的成交量:只用于中间计算
        /// </summary>
        public int PreVol { get; set; }

        /// <summary>
        /// 分钟的tick数量 >3 才会被记录和分发
        /// </summary>
        public int Ticks { get; set; }
    }
}
