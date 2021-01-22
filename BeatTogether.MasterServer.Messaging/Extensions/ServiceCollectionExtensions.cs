using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.MasterServer.Messaging.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMasterServerMessaging(this IServiceCollection services) =>
            services
                .AddCoreMessaging()
                .AddSingleton<IMessageRegistry, HandshakeMessageRegistry>()
                .AddSingleton<IMessageRegistry, UserMessageRegistry>();
    }
}
