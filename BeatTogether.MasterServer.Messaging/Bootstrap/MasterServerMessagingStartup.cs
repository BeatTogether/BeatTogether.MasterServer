using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.MasterServer.Messaging.Bootstrap
{
    public static class MasterServerMessagingStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HandshakeMessageRegistry>();
            services.AddSingleton<UserMessageRegistry>();

            services.AddSingleton<MessageReader<HandshakeMessageRegistry>>();
            services.AddSingleton<MessageReader<UserMessageRegistry>>();
            services.AddSingleton<MessageWriter<HandshakeMessageRegistry>>();
            services.AddSingleton<MessageWriter<UserMessageRegistry>>();
        }
    }
}
