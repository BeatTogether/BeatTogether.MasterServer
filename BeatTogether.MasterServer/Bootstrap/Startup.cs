using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Configuration;
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
                    .Select(hostedService => hostedService.StartAsync(cancellationToken))
            );
            await Task.Delay(-1);
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new MasterServerConfiguration());

            // services.AddSingleton<IServerRepository, ServerRepository>();
            services.AddHostedService<Implementations.MasterServer>();
        }

        public static void ConfigureAppConfiguration(IServiceProvider serviceProvider, IConfiguration builder)
        {
            builder.GetSection("MasterServer").Bind(serviceProvider.GetRequiredService<MasterServerConfiguration>());
        }
    }
}
