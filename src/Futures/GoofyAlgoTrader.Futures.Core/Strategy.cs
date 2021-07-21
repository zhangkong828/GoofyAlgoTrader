using GoofyAlgoTrader.Futures.Core;
using GoofyAlgoTrader.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures
{
    /// <summary>
    /// 策略
    /// </summary>
    public abstract class Strategy : IDisposable
    {
        private readonly ILogger _log;

        public bool IsRunning { get; protected set; }

        public Strategy()
        {
            _log = Log.GetLogger("Exception");
        }

        private List<string> _subscribeList;

        public List<Data> Datas { get; private set; } = new List<Data>();

        #region 事件

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void OnInit();

        /// <summary>
        /// 数据变化
        /// </summary>
        public virtual void OnBar() { }

        #endregion

        /// <summary>
        /// 运行
        /// </summary>
        public void Run()
        {
            if (IsRunning) return;

            try
            {
                _subscribeList = new List<string>();
                this.OnInit();


            }
            catch (Exception ex)
            {
                _log.Error("Strategy.Run", ex);
                return;
            }
            IsRunning = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _log.Release();
            }
        }

        /// <summary>
        /// 订阅行情
        /// </summary>
        /// <param name="instruments">标的代码, 如有多个代码, 用英文逗号隔开</param>
        /// <param name="frequency">频率, 支持 “tick”, “1d”, “15s”, “30s” 等</param>
        public void Subscribe(string instruments, string frequency)
        {
            if (instruments.IsNullOrEmpty()) return;
            var instrumentList = instruments.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in instrumentList)
            {
                if (!_subscribeList.Contains(item))
                {
                    _subscribeList.Add(item);
                }
            }
            
        }


    }
}
