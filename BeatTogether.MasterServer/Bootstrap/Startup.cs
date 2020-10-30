using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Configuration;
using BeatTogether.MasterServer.Data.Abstractions;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Configuration;
using BeatTogether.MasterServer.Data.Implementations;
using BeatTogether.MasterServer.Data.Implementations.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BeatTogether.MasterServer
{
    public static class Startup
    {
        public static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(Environment.GetEnvironmentVariable("BEAT_TOGETHER_CONFIGURATION_BASE_PATH") ?? Directory.GetCurrentDirectory())
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
            var dataConfiguration = new DataConfiguration();
            services.AddSingleton(dataConfiguration);
            services.AddSingleton(dataConfiguration.Redis);
            services.AddSingleton<IConnectionMultiplexerPool, ConnectionMultiplexerPool>();
            services.AddScoped(
                serviceProvider => serviceProvider
                    .GetRequiredService<IConnectionMultiplexerPool>()
                    .GetConnection()
            );
            services.AddSingleton<IServerRepository, ServerRepository>();

            services.AddSingleton<MasterServerConfiguration>();
            services.AddHostedService<Implementations.MasterServer>();
        }

        public static void ConfigureAppConfiguration(IServiceProvider serviceProvider, IConfiguration builder)
        {
            builder.GetSection("Data").Bind(serviceProvider.GetRequiredService<DataConfiguration>());
            builder.GetSection("Data").GetSection("Redis").Bind(serviceProvider.GetRequiredService<RedisConfiguration>());
            builder.GetSection("MasterServer").Bind(serviceProvider.GetRequiredService<MasterServerConfiguration>());
        }
    }
}
