using CSRedis;
using GoofyAlgoTrader.Futures.Core;
using GoofyAlgoTrader.Logging;
using nctp;
using System;
using System.Collections.Concurrent;
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
        private readonly CSRedisClient _redisClient;
        private TradeExt _t;
        private QuoteExt _q;

        public DateTime _tradingDay { get; set; }
        public DateTime _actionDay { get; set; }
        public DateTime _actionDayNext { get; set; }

        private ConcurrentDictionary<string, Bar> _instLastMin = new ConcurrentDictionary<string, Bar>();

        private const double E = 0.000001;
        public long _ticks = 0;
        public long _execTicks = 0;
        public CtpService(CSRedisClient redisClient, Account account)
        {
            _redisClient = redisClient;
            _account = account;
        }

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
                if (DateTime.TryParse(_t.TradingDay, out DateTime tradingDay))
                {
                    _tradingDay = tradingDay;
                    if (tradingDay.DayOfWeek == DayOfWeek.Monday)//周一
                    {
                        _actionDay = tradingDay.AddDays(-3);//上周五
                        _actionDayNext = tradingDay.AddDays(2);//上周六
                    }
                    else
                    {
                        _actionDay = tradingDay.AddDays(-1);//上一天
                        _actionDayNext = tradingDay;//本日
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
            var action = _tradingDay;

            if (DateTime.TryParse(e.Tick.UpdateTime, out DateTime updateTime))
            {
                //夜盘
                if (updateTime.Hour <= 3)
                    action = _actionDayNext;
                else if (updateTime.Hour >= 20)
                    action = _actionDay;
            }

            var minDateTime = new DateTime(action.Year, action.Month, action.Day, updateTime.Hour, updateTime.Minute, 0);
            var minDateTimeStr = minDateTime.ToString("yyyy-MM-dd HH:mm:ss");


            if (!_instLastMin.TryGetValue(e.Tick.InstrumentID, out Bar bar))
            {
                bar = new Bar();
                bar.Id = minDateTimeStr;
                bar.Open = e.Tick.LastPrice;
                bar.High = e.Tick.LastPrice;
                bar.Close = e.Tick.LastPrice;
                bar.Low = e.Tick.LastPrice;
                bar.Volume = 0;
                bar.PreVol = e.Tick.Volume;
                bar.OpenInterestI = e.Tick.OpenInterest;
                bar.TradingDay = int.Parse(_tradingDay.ToString("yyyyMMdd"));
                bar.Ticks = 1;
            }
            else
            {
                if (!DateTime.TryParse(bar.Id, out DateTime barId))
                    return;

                var minDiff = DateTime.Compare(minDateTime, barId);

                if (minDiff < 0)//小于0 旧数据 不处理
                {
                    return;
                }

                if (minDiff > 0)
                {
                    bar.Id = minDateTimeStr;
                    bar.Open = e.Tick.LastPrice;
                    bar.High = e.Tick.LastPrice;
                    bar.Close = e.Tick.LastPrice;
                    bar.Low = e.Tick.LastPrice;
                    bar.PreVol = bar.PreVol + bar.Volume;
                    bar.Volume = e.Tick.Volume - bar.PreVol;
                    bar.OpenInterestI = e.Tick.OpenInterest;
                    bar.Ticks = 1;
                }
                else
                {
                    //数据更新
                    if (e.Tick.LastPrice - bar.High > E)
                        bar.High = e.Tick.LastPrice;
                    if (e.Tick.LastPrice - bar.Low < E)
                        bar.Low = e.Tick.LastPrice;
                    bar.Close = e.Tick.LastPrice;
                    bar.Volume = e.Tick.Volume - bar.PreVol;
                    bar.OpenInterestI = e.Tick.OpenInterest;

                    if (bar.Volume > 0)// 过滤成交量==0的数据
                    {
                        bar.Ticks++;

                        var cacheKey = CacheKey.FuturesInstrumentLastMin(e.Tick.InstrumentID);
                        if (bar.Ticks == 3)// 控制分钟最小tick数量  避免盘歇的数据
                        {
                            _redisClient.RPush(cacheKey, bar);
                        }
                        else if (bar.Ticks > 3)
                        {
                            _redisClient.LSet(cacheKey, -1, bar);
                        }
                    }
                }
            }

            _instLastMin.TryAdd(e.Tick.InstrumentID, bar);
            _execTicks++;
        }
    }
}
