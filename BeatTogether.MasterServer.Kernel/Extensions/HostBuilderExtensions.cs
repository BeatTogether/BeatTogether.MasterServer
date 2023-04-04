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
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernal;
using BeatTogether.MasterServer.Interface.ApiInterface;
using BeatTogether.MasterServer.Kernal.Abstractions;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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
                .ConfigureWebHostDefaults(webHostBuilder =>
                    webHostBuilder
                        .ConfigureServices((hostBuilderContext, services) =>
                            services
                                .AddCoreSecurity()
                                .AddServiceKernel<IApiInterface, ApiInterface>()
                                .AddMasterServerMessaging()
                                .AddAutoMapper(configuration =>
                                {
                                    configuration.CreateMap<BeatTogether.MasterServer.Messaging.Models.GameplayServerConfiguration,
                                        DedicatedServer.Interface.Models.GameplayServerConfiguration>();
                                })
                                .AddConfiguration<MasterServerConfiguration>("MasterServer")
                                .AddTransient<SecureRandom>()
                                .AddTransient<RNGCryptoServiceProvider>()
                                .AddSingleton<HttpClient>()
                                .AddSingleton<ICookieProvider, CookieProvider>()
                                .AddSingleton<IRandomProvider, RandomProvider>()
                                .AddSingleton<IServerCodeProvider, ServerCodeProvider>()
                                .AddSingleton<ISecretProvider, SecretProvider>()
                                .AddScoped<IHandshakeService, HandshakeService>()
                                .AddScoped<IUserService, UserService>()
                                .AddSingleton<IMasterServerSessionService, MasterServerSessionService>()
                                .AddSingleton<INodeRepository, NodeRepository>()
                                .AddSingleton<MasterServerMessageSource>()
                                .AddSingleton<MasterServerMessageDispatcher>()
                                .AddServiceClient<IMatchmakingService>()
                                .AddHostedService<DedicatedServerEventHandler>()
                                .AddHostedService<MasterServer>()
                                .AddHostedService<MasterServerSessionTickService>()
                                .AddHostedService<HandshakeMessageHandler>()
                                .AddHostedService<UserMessageHandler>()
                                .AddOptions()
                                .AddControllers()
                                .AddNewtonsoftJson()
                        )
                        .Configure(applicationBuilder =>
                            applicationBuilder
                                .UseRouting()
                                .UseEndpoints(endPointRouteBuilder => endPointRouteBuilder.MapControllers())
                        )
                );
    }
}
