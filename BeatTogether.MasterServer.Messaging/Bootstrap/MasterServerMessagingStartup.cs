using System.Security.Cryptography;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.MasterServer.Messaging.Bootstrap
{
    public static class MasterServerMessagingStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<RNGCryptoServiceProvider>();
            services.AddTransient(serviceProvider =>
                new AesCryptoServiceProvider()
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None
                }
            );

            services.AddSingleton<IMessageRegistry, HandshakeMessageRegistry>();
            services.AddSingleton<IMessageRegistry, UserMessageRegistry>();

            services.AddSingleton<IMessageReader, MessageReader>();
            services.AddSingleton<IMessageWriter, MessageWriter>();
            services.AddSingleton<IEncryptedMessageReader, EncryptedMessageReader>();
            services.AddSingleton<IEncryptedMessageWriter, EncryptedMessageWriter>();
        }
    }
}
