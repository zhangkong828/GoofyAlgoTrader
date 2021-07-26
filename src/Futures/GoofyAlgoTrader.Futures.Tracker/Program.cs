using GoofyAlgoTrader.Configurations;
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
            var host = new HostBuilder()
                        .UseConsoleLifetime()
                        .ConfigureAppConfiguration((context, configurationBuilder) =>
                        {
                            configurationBuilder.AddConfigFile("GoofyAlgoTrader.json", optional: true, reloadOnChange: true);
                        })
                        .ConfigureServices((context, services) =>
                        {
                            services.UseGoofyAlgoTrader();

                            services.AddHostedService<TrackerService>();
                        })
                        .Build();

            host.Run();
        }
    }
}
