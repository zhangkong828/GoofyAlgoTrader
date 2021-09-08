using CSRedis;
using GoofyAlgoTrader.Logging;
using nctp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly List<string> _products;

        private TradeExt _t;
        private QuoteExt _q;

        public DateTime _tradingDay { get; set; }
        public DateTime _actionDay { get; set; }
        public DateTime _actionDayNext { get; set; }

        private ConcurrentDictionary<string, Bar> _instLastMin;
        private ConcurrentDictionary<string, ExchangeStatusType> _mapInstrumentStatus;

        private const double E = 0.000001;
        private long _ticks = 0;
        private long _execTicks = 0;
        private string _showTime;

        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public CtpService(CSRedisClient redisClient, Account account, List<string> products)
        {
            _redisClient = redisClient;
            _account = account;
            _products = products;
        }

        public void Run()
        {
            Task.Factory.StartNew(() =>
            {
                StarTrade();
            });

            Task.Factory.StartNew(() =>
            {
                StartQuote();
            });

            int waitLoginSecond = 60;
            while (true)
            {
                //等待登录
                if (_t != null && _t.IsLogin)
                {
                    var cntNotClose = 0;
                    var cntTrading = 0;
                    // 每分钟判断一次
                    Thread.Sleep(1000 * 60);
                    foreach (var item in _t.DicExcStatus)
                    {
                        var status = item.Value;
                        if (status != ExchangeStatusType.Closed)
                            cntNotClose++;
                        if (status == ExchangeStatusType.Trading)
                            cntTrading++;
                    }

                    _log.Info($"NotClose:{cntNotClose} Trading:{cntTrading} {_showTime}->有效/全部:{_execTicks}/{_ticks}");

                    // 全关闭
                    if (cntNotClose == 0)
                    {
                        // 保存分钟数据
                        SaveDB();
                        break;
                    }

                    //3点前全都为非交易状态
                    if (DateTime.Now.Hour <= 3 && cntTrading == 0)
                    {
                        _log.Info("夜盘结束");
                        break;
                    }
                }
                else
                {
                    if (waitLoginSecond <= 0)
                    {
                        _log.Error($"trade:wait login fail");
                        break;
                    }

                    Thread.Sleep(1000);
                    waitLoginSecond--;
                    continue;
                }
            }

            Stop();
        }

        public void Stop()
        {
            try
            {
                _log.Info("stop ctp service");
                if (_t != null)
                    _t.ReqUserLogout();
                if (_q != null)
                    _q.ReqUserLogout();

            }
            catch { }
        }

        public void FlushRedis()
        {
            var keys = _redisClient.Keys($"{CacheKey.PrefixKey}Futures:*");
            _redisClient.Del(keys);
        }

        private void StarTrade()
        {
            _mapInstrumentStatus = new ConcurrentDictionary<string, ExchangeStatusType>();

            _log.Debug("trade:connect ...");
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
            _t.OnRspUserLogout += _t_OnRspUserLogout;
            _t.ReqConnect();
        }

        private void _t_OnRspUserLogout(object sender, IntEventArgs e)
        {
            _log.Info($"trade:logout {e.Value}");
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
                _log.Info("trade:user login success");
                _instLastMin = new ConcurrentDictionary<string, Bar>();
                if (DateTime.TryParseExact(_t.TradingDay, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime tradingDay))
                {
                    _tradingDay = tradingDay;
                    var preDay = _redisClient.HGet(CacheKey.FuturesHashConfig(), "tradingday");
                    if (!preDay.EqualIgnoreCase(_t.TradingDay))
                    {
                        FlushRedis();
                        _ticks = 0;
                        _execTicks = 0;
                        _redisClient.HSet(CacheKey.FuturesHashConfig(), "tradingday", _t.TradingDay);
                    }

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
            }
            else
            {
                _log.Error($"trade:user login fail {e.ErrorID}={e.ErrorMsg}");
            }

            _autoResetEvent.Set();
        }

        private void StartQuote()
        {
            _log.Info("waiting trade login");
            _autoResetEvent.WaitOne();

            _log.Debug("quote:connect ...");
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
            _log.Info($"quote:logout {e.Value}");
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
                _log.Info($"quote:user login success {_q.Investor}");
                _mapInstrumentStatus = new ConcurrentDictionary<string, ExchangeStatusType>();
                int count = 0;
                foreach (var item in _t.DicInstrumentField)
                {
                    if (item.Value.ProductID.IsNullOrEmpty()) continue;

                    try
                    {
                        //最新k线数据
                        var instrumentId = item.Key;
                        var bars = _redisClient.LRange<Bar>($"{CacheKey.FuturesInstrumentLastMin()}:{instrumentId}", -1, -1);
                        if (bars != null && bars.Length > 0)
                        {
                            _instLastMin.TryAdd(instrumentId, bars[0]);
                        }

                        //更新合约状态
                        if (_t.DicExcStatus.TryGetValue(item.Value.ProductID, out ExchangeStatusType status))
                        {
                            _mapInstrumentStatus.TryAdd(instrumentId, status);
                        }

                        //订阅列表
                        if (_products != null && _products.Count > 0)
                        {
                            int index = -1;
                            for (int i = 0; i < _products.Count; i++)
                            {
                                if (_products[i].EqualIgnoreCase(instrumentId))
                                {
                                    index = i;
                                    break;
                                }
                            }
                            //不在列表
                            if (index == -1)
                            {
                                continue;
                            }
                        }

                        _q.ReqSubscribeMarketData(instrumentId);
                        //防止数量太多
                        if (count % 5000 == 0)
                        {
                            var sleep = Utils.RandomNumber() * 200;
                            Thread.Sleep(sleep);
                        }
                        count++;
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"quote.OnRspUserLogin: {item.Key}", ex);
                    }

                }

                _log.Info($"quote:subscript instrument count: {count}");
            }
            else
            {
                _log.Error($"quote:user login fail {e.Value}");
                _q.ReqUserLogout();
            }
        }
        private void _q_OnRtnTick(object sender, TickEventArgs e)
        {
            Interlocked.Increment(ref _ticks);

            Task.Factory.StartNew(() =>
            {
                if (!_mapInstrumentStatus.TryGetValue(e.Tick.InstrumentID, out ExchangeStatusType statusType))
                    return;

                if (statusType != ExchangeStatusType.Trading)
                    return;

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
                _showTime = minDateTimeStr;

                _instLastMin.AddOrUpdate(e.Tick.InstrumentID, key =>
                {
                    var bar = new Bar();
                    bar.Id = minDateTimeStr;
                    bar.Open = e.Tick.LastPrice;
                    bar.High = e.Tick.LastPrice;
                    bar.Close = e.Tick.LastPrice;
                    bar.Low = e.Tick.LastPrice;
                    bar.Volume = 0;
                    bar.PreVol = e.Tick.Volume;
                    bar.OpenInterest = e.Tick.OpenInterest;
                    bar.TradingDay = _tradingDay.ToString("yyyyMMdd");
                    bar.Ticks = 1;
                    return bar;
                }, (key, bar) =>
                {
                    if (!DateTime.TryParse(bar.Id, out DateTime barId))
                        return bar;

                    var minDiff = DateTime.Compare(minDateTime, barId);

                    if (minDiff < 0)//小于0 旧数据 不处理
                    {
                        return bar;
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
                        bar.OpenInterest = e.Tick.OpenInterest;
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
                        bar.OpenInterest = e.Tick.OpenInterest;

                        if (bar.Volume > 0)// 过滤成交量==0的数据
                        {
                            bar.Ticks++;

                            var cacheKey = $"{CacheKey.FuturesInstrumentLastMin()}:{e.Tick.InstrumentID}";
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
                    return bar;
                });

                Interlocked.Increment(ref _execTicks);
            });

        }

        private void SaveDB()
        {
            int count = 0;
            try
            {
                var keys = _redisClient.Keys($"{CacheKey.FuturesInstrumentLastMin()}*");
                if (keys != null && keys.Length > 0)
                {
                    foreach (var key in keys)
                    {
                        var mins = _redisClient.LRange<Bar>(key, 0, -1);
                        var preMin = DateTime.MinValue;
                        foreach (var bar in mins)
                        {
                            if (DateTime.TryParse(bar.Id, out DateTime barMin) && DateTime.Compare(barMin, preMin) > 0)
                            {
                                var model = new MinBarModel()
                                {
                                    DateTime = bar.Id,
                                    Instrument = key.TrimStart($"{CacheKey.FuturesInstrumentLastMin()}:"),
                                    Open = bar.Open,
                                    High = bar.High,
                                    Low = bar.Low,
                                    Close = bar.Close,
                                    Volume = bar.Volume,
                                    OpenInterest = bar.OpenInterest,
                                    TradingDay = bar.TradingDay
                                };
                                if (DbService.InsertBar(model))
                                    count++;
                            }
                            preMin = barMin;
                        }
                    }
                    FlushRedis();
                }

            }
            catch (Exception ex)
            {
                _log.Error($"入库异常,30分钟后清库", ex);
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000 * 60 * 30);
                    FlushRedis();
                });
            }
            finally
            {
                _log.Info($"入库:{count}");
            }
        }
    }
}
