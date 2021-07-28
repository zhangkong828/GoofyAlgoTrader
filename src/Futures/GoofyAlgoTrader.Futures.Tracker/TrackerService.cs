using GoofyAlgoTrader.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class TrackerService : IHostedService
    {
        private readonly ILogger _log = Log.GetLogger();
        private readonly CtpService _ctpService;

        private readonly List<string> _tradingDays;

        public TrackerService()
        {
            var account = Config.Get<Account>("Account");
            _ctpService = new CtpService(account);

            //获取 交易日历
            _tradingDays = new List<string>();
            foreach (var item in File.ReadLines("calendar.csv"))
            {
                if (item.IsNullOrEmpty()) continue;
                var cols = item.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (cols.Length == 2 && cols[1] == "true")
                {
                    var tradingDay = cols[0].Trim('\"');
                    _tradingDays.Add(tradingDay);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var currentDate = DateTime.Today;
                for (int i = 0; i < _tradingDays.Count; i++)
                {
                    if (!DateTime.TryParse(_tradingDays[i], out DateTime tradingDay)) continue;
                    if (DateTime.Compare(tradingDay, currentDate) < 0) continue;


                    //8:45之前等待
                    var currentNow = DateTime.Now;
                    var startTime = tradingDay.AddHours(8).AddMinutes(45);
                    if (currentNow < startTime)
                    {
                        _log.Info($"waiting for trading start at {startTime}");
                        var waitingTime = startTime - currentNow;
                        Task.Delay((int)waitingTime.TotalMilliseconds, cancellationToken);
                    }
                    // 15:00前开启
                    currentNow = DateTime.Now;
                    startTime = tradingDay.AddHours(15);
                    if (currentNow < startTime)
                    {
                        _ctpService.Run();
                    }

                    //当日有夜盘(下一交易日在3天内)
                    if (!DateTime.TryParse(_tradingDays[i + 1], out DateTime nextTradingDay)) continue;
                    if (nextTradingDay < DateTime.Today.AddDays(3))
                    {
                        //20:45
                        currentNow = DateTime.Now;
                        startTime = tradingDay.AddHours(20).AddMinutes(45);
                        if (currentNow < startTime)
                        {
                            _log.Info($"waiting for night open at {startTime}");
                            var waitingTime = startTime - currentNow;
                            Task.Delay((int)waitingTime.TotalMilliseconds, cancellationToken);
                        }

                        _ctpService.Run();
                    }
                }

            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ctpService.Stop();
            return Task.CompletedTask;
        }
    }
}
