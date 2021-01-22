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
                services.AddStackExchangeRedis();
                if (hostBuilderContext.Configuration.GetSection("Redis") is not null)
                    services.AddScoped<IServerRepository, ServerRepository>();
                else
                    services.AddScoped<IServerRepository, MemoryServerRepository>();
            });
    }
}
