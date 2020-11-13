using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Data.Abstractions;
using BeatTogether.MasterServer.Data.Bootstrap;
using BeatTogether.MasterServer.Data.Configuration;
using BeatTogether.MasterServer.Kernel.Bootstrap;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Messaging.Bootstrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.MasterServer
{
    public static class MasterServerStartup
    {
        public static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json")
                .AddEnvironmentVariables();
            var configuration = configurationBuilder.Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(configuration)
                .CreateLogger();

            var services = new ServiceCollection();
            services.AddSingleton(services);
            services.AddSingleton(configuration);
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();
            ConfigureAppConfiguration(serviceProvider, configuration);

            // Warmup the Redis connection pool
            serviceProvider.GetRequiredService<IConnectionMultiplexerPool>();

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            await Task.WhenAll(
                serviceProvider
                    .GetServices<IHostedService>()
                    .Select(hostedService =>
                    {
                        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                        {
                            hostedService.StopAsync(cancellationToken).Wait();
                            Task.Delay(1000).Wait();
                        };
                        return hostedService.StartAsync(cancellationToken);
                    })
            );
            await Task.Delay(-1);
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            MasterServerDataStartup.ConfigureServices(services);
            MasterServerMessagingStartup.ConfigureServices(services);
            MasterServerKernelStartup.ConfigureServices(services);
        }

        public static void ConfigureAppConfiguration(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            configuration.GetSection("Data").Bind(serviceProvider.GetRequiredService<DataConfiguration>());
            configuration.GetSection("Data").GetSection("Redis").Bind(serviceProvider.GetRequiredService<RedisConfiguration>());
            configuration.GetSection("MasterServer").Bind(serviceProvider.GetRequiredService<MasterServerConfiguration>());
            configuration.GetSection("Messaging").Bind(serviceProvider.GetRequiredService<MessagingConfiguration>());
        }
    }
}
