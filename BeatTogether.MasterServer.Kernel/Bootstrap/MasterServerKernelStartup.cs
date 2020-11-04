using BeatTogether.MasterServer.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.MasterServer.Kernel.Bootstrap
{
    public static class MasterServerKernelStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MasterServerConfiguration>();

            services.AddHostedService<Implementations.MasterServer>();
        }
    }
}
