using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using BeatTogether.MasterServer.Kernel.Implementations.Security;
using BeatTogether.MasterServer.Kernel.Implementations.Sessions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Security;

namespace BeatTogether.MasterServer.Kernel.Bootstrap
{
    public static class MasterServerKernelStartup
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services) =>
            services
                .AddSingleton(
                    hostBuilderContext
                        .Configuration
                        .GetSection("MasterServer")
                        .Get<MasterServerConfiguration>()
                )
                .AddSingleton(
                    hostBuilderContext
                        .Configuration
                        .GetSection("Messaging")
                        .Get<MessagingConfiguration>()
                )
                .AddSingleton(
                    hostBuilderContext
                        .Configuration
                        .GetSection("SessionLifetime")
                        .Get<SessionLifetimeConfiguration>()
                )

                .AddTransient<SecureRandom>()
                .AddTransient<RNGCryptoServiceProvider>()

                .AddSingleton<ICookieProvider, CookieProvider>()
                .AddSingleton<IRandomProvider, RandomProvider>()
                .AddSingleton<ICertificateProvider, CertificateProvider>()
                .AddSingleton<IServerCodeProvider, ServerCodeProvider>()

                .AddSingleton<IDiffieHellmanService, DiffieHellmanService>()
                .AddSingleton<ICertificateSigningService, CertificateSigningService>()

                .AddSingleton<IMessageDispatcher, MessageDispatcher>()

                .AddSingleton<HandshakeMessageReceiver>()
                .AddSingleton<UserMessageReceiver>()

                .AddSingleton<ISessionService, SessionService>()
                .AddSingleton<IMultipartMessageService, MultipartMessageService>()
                .AddScoped<IHandshakeService, HandshakeService>()
                .AddScoped<IUserService, UserService>()

                .AddHostedService<SessionTickService>()
                .AddHostedService<Implementations.MasterServer>();
    }
}
