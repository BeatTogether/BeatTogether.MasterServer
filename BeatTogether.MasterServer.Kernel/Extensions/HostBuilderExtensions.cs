using System.Security.Cryptography;
using Autobus;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using BeatTogether.MasterServer.Kernel.Implementations.Sessions;
using BeatTogether.MasterServer.Messaging.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Security;

namespace BeatTogether.Extensions
{
    using MasterServer = MasterServer.Kernel.Implementations.MasterServer;

    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseMasterServerKernel(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .UseAutobus()
                .UseMasterServerData()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddCoreSecurity()
                        .AddMasterServerMessaging()
                        .AddConfiguration<MasterServerConfiguration>("MasterServer")
                        .AddServiceClient<IRelayServerService>()
                        .AddTransient<SecureRandom>()
                        .AddTransient<RNGCryptoServiceProvider>()
                        .AddSingleton<ICookieProvider, CookieProvider>()
                        .AddSingleton<IRandomProvider, RandomProvider>()
                        .AddSingleton<IServerCodeProvider, ServerCodeProvider>()
                        .AddScoped<IHandshakeService, HandshakeService>()
                        .AddScoped<IUserService, UserService>()
                        .AddSingleton<IMasterServerSessionService, MasterServerSessionService>()
                        .AddSingleton<MasterServerMessageSource>()
                        .AddSingleton<MasterServerMessageDispatcher>()
                        .AddHostedService<MasterServer>()
                        .AddHostedService<MasterServerSessionTickService>()
                        .AddHostedService<HandshakeMessageHandler>()
                        .AddHostedService<UserMessageHandler>()
                );
    }
}
