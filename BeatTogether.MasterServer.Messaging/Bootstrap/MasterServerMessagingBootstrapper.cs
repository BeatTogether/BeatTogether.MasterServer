using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Core.Messaging.Bootstrap;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.MasterServer.Messaging.Bootstrap
{
    public static class MasterServerMessagingBootstrapper
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            CoreMessagingBootstrapper.ConfigureServices(hostBuilderContext, services);
            services.AddSingleton<IMessageRegistry, HandshakeMessageRegistry>();
            services.AddSingleton<IMessageRegistry, UserMessageRegistry>();
        }
    }
}
