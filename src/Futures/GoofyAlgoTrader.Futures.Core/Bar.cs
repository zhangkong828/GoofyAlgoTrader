using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GoofyAlgoTrader.Futures.Core
{
    public class Bar
    {
        /// <summary>
        /// 时间
        /// </summary>
        [ReadOnly(true)]
        public DateTime D { get; set; }

        /// <summary>
        /// 交易日
        /// </summary>
        public int TradingDay { get; set; }

        /// <summary>
        /// 开盘价
        /// </summary>
        [Description("开盘价"), ReadOnly(true)]
        public double O { get; set; }

        /// <summary>
        /// 最高价
        /// </summary>
        [Description("最高价"), ReadOnly(true)]
        public double H { get; set; }

        /// <summary>
        /// 最低价
        /// </summary>
        [Description("最低价"), ReadOnly(true)]
        public double L { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        [Description("收盘价"), ReadOnly(true)]
        public double C { get; set; }

        /// <summary>
        /// 成交量
        /// </summary>
        [Description("成交量"), ReadOnly(true)]
        public double V { get; set; }

        /// <summary>
        /// 持仓量
        /// </summary>
        [Description("持仓量"), ReadOnly(true)]
        public double I { get; set; }

        /// <summary>
        /// 前Bar的成交量:只用于中间计算
        /// </summary>
        internal double PreVol { get; set; }
    }
}
