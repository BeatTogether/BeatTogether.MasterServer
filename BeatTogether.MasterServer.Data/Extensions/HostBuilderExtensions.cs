using System.Linq;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Implementations.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseMasterServerData(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices((hostBuilderContext, services) =>
            {
                var isRedisConfigured = hostBuilderContext.Configuration
                    .GetChildren()
                    .Any(child => child.Key == "Redis");
                if (isRedisConfigured)
                    services
                        .AddStackExchangeRedis()
                        .AddScoped<IServerRepository, ServerRepository>();
                else
                    services.AddScoped<IServerRepository, MemoryServerRepository>();
            });
    }
}
