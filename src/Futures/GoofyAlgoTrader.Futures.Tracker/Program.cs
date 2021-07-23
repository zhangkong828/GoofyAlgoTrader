using GoofyAlgoTrader.Configurations;
using GoofyAlgoTrader.Futures.Tracker.CTP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GoofyAlgoTrader.Futures.Tracker
{
    class Program
    {
        static void Main(string[] args)
        {
            //var host = new HostBuilder()
            //            .UseConsoleLifetime()
            //            .ConfigureAppConfiguration((context, configurationBuilder) =>
            //            {
            //                configurationBuilder.AddConfigFile("GoofyAlgoTrader.json", optional: true, reloadOnChange: true);
            //            })
            //            .ConfigureServices((context, services) =>
            //            {
            //                services.UseGoofyAlgoTrader();

            //                services.AddHostedService<TrackerService>();
            //            })
            //            .Build();

            //host.Run();


            var configurationBuilder = new ConfigurationBuilder()
           .SetBasePath(AppContext.BaseDirectory)
           .AddJsonFile("GoofyAlgoTrader.json", optional: true, reloadOnChange: true);
            Config.Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();         
            services.UseGoofyAlgoTrader();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var account = Config.Get<Account>("Account");
            var ctpService = new CtpService(account);
            ctpService.Run();
        }
    }
}
