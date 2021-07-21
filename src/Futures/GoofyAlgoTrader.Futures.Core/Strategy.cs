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
    public abstract class Strategy: IDisposable
    {
        private readonly ILogger _log;

        public string Name { get; set; }
        public bool IsRunning { get; protected set; }

        public Strategy()
        {
            _log= Log.GetLogger("Exception");
        }


        public List<Data> Datas { get; private set; } = new List<Data>();

        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 数据变化
        /// </summary>
        public abstract void OnBarUpdate();

        public void Run()
        {
            if (IsRunning) return;

            try
            {
                this.Initialize();
            }
            catch (Exception ex)
            {
                _log.Error("Strategy.Initialize", ex);
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
    }
}
