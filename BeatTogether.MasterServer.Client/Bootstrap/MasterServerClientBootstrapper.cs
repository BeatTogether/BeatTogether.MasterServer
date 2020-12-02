using BeatTogether.Core.Hosting.Extensions;
using BeatTogether.MasterServer.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.MasterServer.Messaging.Bootstrap
{
    public static class MasterServerClientBootstrapper
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            MasterServerMessagingBootstrapper.ConfigureServices(hostBuilderContext, services);
            services.AddConfiguration<MasterServerClientConfiguration>(hostBuilderContext.Configuration, "MasterServerClient");
        }
    }
}
