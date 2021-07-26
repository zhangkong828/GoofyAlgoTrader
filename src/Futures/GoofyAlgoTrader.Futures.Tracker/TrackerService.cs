using GoofyAlgoTrader.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoofyAlgoTrader.Futures.Tracker
{
    public class TrackerService : IHostedService
    {
        private readonly ILogger _log = Log.GetLogger();
        private readonly CtpService _ctpService;

        public TrackerService()
        {
            var account = Config.Get<Account>("Account");
            _ctpService = new CtpService(account);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ctpService.Run();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _ctpService.Stop();
            return Task.CompletedTask;
        }
    }
}
