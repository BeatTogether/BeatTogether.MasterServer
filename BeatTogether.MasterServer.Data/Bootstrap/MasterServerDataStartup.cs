using BeatTogether.MasterServer.Data.Abstractions;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Configuration;
using BeatTogether.MasterServer.Data.Implementations;
using BeatTogether.MasterServer.Data.Implementations.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.MasterServer.Data.Bootstrap
{
    public static class MasterServerDataStartup
    {
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
        }
    }
}
