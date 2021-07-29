using GoofyAlgoTrader.Futures.Core;
using GoofyAlgoTrader.Logging;
using nctp;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class TradeExt : CTPTrade
    {
        public TradeExt()
        {
        }
    }

    public class QuoteExt : CTPQuote
    {
        public QuoteExt()
        {

        }
    }

    public class CtpService
    {
        private readonly ILogger _log = Log.GetLogger("CtpService");

        private readonly Account _account;
        private TradeExt _t;
        private CTPQuote _q;

        public DateTime TradingDay { get; set; }
        public DateTime ActionDay { get; set; }
        public DateTime ActionDayNext { get; set; }
        public CtpService(Account account)
        {
            _account = account;
        }

        public Action<MarketData> OnTick { get; set; }

        public void Run()
        {
            _t = new TradeExt()
            {
                FrontAddr = _account.TradeFrontAddr,
                Broker = _account.Broker,
                Investor = _account.Investor,
                Password = _account.Password,
                ProductInfo = _account.ProductInfo,
                AppID = _account.AppID,
                AuthCode = _account.AuthCode,
            };
            _t.OnFrontConnected += _t_OnFrontConnected;
            _t.OnRspUserLogin += _t_OnRspUserLogin;
            _t.ReqConnect();
        }

        public void Stop()
        {
            if (_t != null)
                _t.ReqUserLogout();
            if (_q != null)
                _q.ReqUserLogout();
        }

        private void _t_OnFrontConnected(object sender, EventArgs e)
        {
            _log.Debug("trade:connected");
            _t.ReqUserLogin();
        }

        private void _t_OnRspUserLogin(object sender, ErrorEventArgs e)
        {
            if (e.ErrorID == 0)
            {
                _log.Debug("trade:user login success");

                if (DateTime.TryParse(_t.TradingDay, out DateTime TradingDay))
                {
                    if (TradingDay.DayOfWeek == DayOfWeek.Monday)//周一
                    {
                        ActionDay = TradingDay.AddDays(-3);//上周五
                        ActionDayNext = TradingDay.AddDays(2);//上周六
                    }
                    else
                    {
                        ActionDay = TradingDay.AddDays(-1);//上一天
                        ActionDayNext = TradingDay;//本日
                    }
                }

                StartQuote();
            }
            else
            {
                _log.Error($"trade:user login fail {e.ErrorID}={e.ErrorMsg}");
            }
        }

        private void StartQuote()
        {
            _q = new QuoteExt()
            {
                FrontAddr = _account.MarketFrontAddr,
                Broker = _account.Broker,
            };
            _q.OnFrontConnected += _q_OnFrontConnected;
            _q.OnRspUserLogin += _q_OnRspUserLogin;
            _q.OnRspUserLogout += _q_OnRspUserLogout;
            _q.OnRtnTick += _q_OnRtnTick;
            _q.OnRtnError += _q_OnRtnError;

            _q.ReqConnect();
        }

        private void _q_OnRtnError(object sender, ErrorEventArgs e)
        {
            _log.Error($"quote:error {e.ErrorID}={e.ErrorMsg}");
        }

        private void _q_OnRspUserLogout(object sender, IntEventArgs e)
        {
            _log.Debug($"quote:logout {e.Value}");
        }

        private void _q_OnFrontConnected(object sender, EventArgs e)
        {
            _log.Debug("quote:connected");
            _q.ReqUserLogin();
        }

        private void _q_OnRspUserLogin(object sender, IntEventArgs e)
        {
            if (e.Value == 0)
            {
                _log.Debug($"quote:user login success {_q.Investor}");

                foreach (var item in _t.DicInstrumentField)
                {
                    if (item.Value.ProductID.IsNullOrEmpty()) continue;

                }
                _q.ReqSubscribeMarketData("rb2110");
            }
            else
            {
                _log.Error($"quote:user login fail {e.Value}");
                _q.ReqUserLogout();
            }
        }
        private void _q_OnRtnTick(object sender, TickEventArgs e)
        {
            var action = TradingDay;

            if (DateTime.TryParse(e.Tick.UpdateTime, out DateTime updateTime))
            {
                //夜盘
                if (updateTime.Hour <= 3)
                    action = ActionDayNext;
                else if (updateTime.Hour >= 20)
                    action = ActionDay;
            }

            var minDateTime = new DateTime(action.Year, action.Month, action.Day, updateTime.Hour, updateTime.Minute, 0);


            OnTick?.Invoke(e.Tick);
        }
    }
}
