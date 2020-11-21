using System.Security.Cryptography;
using BeatTogether.MasterServer.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Abstractions.Registries;
using BeatTogether.MasterServer.Messaging.Implementations;
using BeatTogether.MasterServer.Messaging.Implementations.Registries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.MasterServer.Messaging.Bootstrap
{
    public static class MasterServerMessagingStartup
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services) =>
            services
                .AddTransient<RNGCryptoServiceProvider>()
                .AddTransient(serviceProvider =>
                    new AesCryptoServiceProvider()
                    {
                        Mode = CipherMode.CBC,
                        Padding = PaddingMode.None
                    }
                )

                .AddSingleton<IMessageRegistry, HandshakeMessageRegistry>()
                .AddSingleton<IMessageRegistry, UserMessageRegistry>()

                .AddSingleton<IMessageReader, MessageReader>()
                .AddSingleton<IMessageWriter, MessageWriter>()
                .AddSingleton<IEncryptedMessageReader, EncryptedMessageReader>()
                .AddSingleton<IEncryptedMessageWriter, EncryptedMessageWriter>();
    }
}
