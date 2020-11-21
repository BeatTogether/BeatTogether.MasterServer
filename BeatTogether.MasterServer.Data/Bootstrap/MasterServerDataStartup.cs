using BeatTogether.MasterServer.Data.Abstractions;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Configuration;
using BeatTogether.MasterServer.Data.Implementations;
using BeatTogether.MasterServer.Data.Implementations.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.MasterServer.Data.Bootstrap
{
    public static class MasterServerDataStartup
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services) =>
            services
                .AddSingleton(
                    hostBuilderContext
                        .Configuration
                        .GetSection("Data")
                        .Get<DataConfiguration>()
                )
                .AddSingleton(
                    hostBuilderContext
                        .Configuration
                        .GetSection("Data:Redis")
                        .Get<RedisConfiguration>()
                )
                .AddSingleton<IConnectionMultiplexerPool, ConnectionMultiplexerPool>()
                .AddScoped(serviceProvider =>
                    serviceProvider
                        .GetRequiredService<IConnectionMultiplexerPool>()
                        .GetConnection()
                )
                .AddScoped<ISessionRepository, SessionRepository>()
                .AddScoped<IServerRepository, ServerRepository>();
    }
}
