using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Core
{
    /// <summary>
    /// 合约信息
    /// </summary>
    public class InstrumentInfo
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        public string InstrumentID;

        /// <summary>
        /// 产品代码
        /// </summary>
        public string ProductID;

        /// <summary>
        /// 合约数量乘数
        /// </summary>
        public int VolumeMultiple;

        /// <summary>
        /// 最小变动价位
        /// </summary>
        public double PriceTick;
    }
}
