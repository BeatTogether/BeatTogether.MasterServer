using BeatTogether.Core.Data.Bootstrap;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Data.Implementations.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.MasterServer.Data.Bootstrap
{
    public static class MasterServerDataBootstrapper
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            CoreDataBootstrapper.ConfigureServices(hostBuilderContext, services);
            services.AddScoped<IServerRepository, ServerRepository>();
        }
    }
}
