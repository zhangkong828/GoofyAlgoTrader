using GoofyAlgoTrader.Logging;
using nctp;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoofyAlgoTrader.Futures.Tracker.CTP
{
    public class CtpService
    {
        private readonly ILogger _log = Log.GetLogger("CtpService");

        private readonly Account _account;
        private TradeExt _t;
        private CTPQuote _q;
        public CtpService(Account account)
        {
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
            _t.ReqUserLogout();
            _q.ReqUserLogout();
        }

        private void _t_OnFrontConnected(object sender, EventArgs e)
        {
            _log.Debug("t:connected");
            _t.ReqUserLogin();
        }

        private void _t_OnRspUserLogin(object sender, ErrorEventArgs e)
        {
            if (e.ErrorID == 0)
            {
                _log.Debug("t:user login success");
                StartQuote();
            }
            else
            {
                _log.Error($"t:user login fail {e.ErrorID}={e.ErrorMsg}");
            }
        }

        private void StartQuote()
        {
            foreach (var v in _t.DicPositionField.Values)
            {
                _log.Info($"posi:{v.InstrumentID}\t{v.Direction}\t{v.Price}\t{v.Position}");
            }
            foreach (var v in _t.DicExcStatus)
            {
                _log.Info($"{v.Key}:{v.Value}");
            }

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
            _log.Error($"q:error {e.ErrorID}={e.ErrorMsg}");
        }

        private void _q_OnRspUserLogout(object sender, IntEventArgs e)
        {
            _log.Debug($"q:logout {e.Value}");
        }

        private void _q_OnFrontConnected(object sender, EventArgs e)
        {
            _log.Debug("q:connected");
            _q.ReqUserLogin();
        }

        private void _q_OnRspUserLogin(object sender, IntEventArgs e)
        {
            if (e.Value == 0)
            {
                _log.Debug($"t:user login success {_q.Investor}");

                //_q.ReqSubscribeMarketData()
            }
            else
            {
                _log.Error($"t:user login fail {e.Value}");
                _q.ReqUserLogout();
            }
        }
        private void _q_OnRtnTick(object sender, TickEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
