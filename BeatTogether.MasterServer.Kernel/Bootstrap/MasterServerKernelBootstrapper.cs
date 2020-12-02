using System.Security.Cryptography;
using BeatTogether.Core.Hosting.Extensions;
using BeatTogether.Core.Security.Bootstrap;
using BeatTogether.MasterServer.Data.Bootstrap;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using BeatTogether.MasterServer.Kernel.Implementations.Sessions;
using BeatTogether.MasterServer.Messaging.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Security;

namespace BeatTogether.MasterServer.Kernel.Bootstrap
{
    public static class MasterServerKernelBootstrapper
    {
        public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
        {
            CoreSecurityBootstrapper.ConfigureServices(hostBuilderContext, services);
            MasterServerMessagingBootstrapper.ConfigureServices(hostBuilderContext, services);
            MasterServerDataBootstrapper.ConfigureServices(hostBuilderContext, services);

            services.AddConfiguration<MasterServerConfiguration>(hostBuilderContext.Configuration, "MasterServer");

            services.AddTransient<SecureRandom>();
            services.AddTransient<RNGCryptoServiceProvider>();

            services.AddSingleton<ICookieProvider, CookieProvider>();
            services.AddSingleton<IRandomProvider, RandomProvider>();
            services.AddSingleton<IServerCodeProvider, ServerCodeProvider>();

            services.AddScoped<IHandshakeService, HandshakeService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDedicatedServerService, DedicatedServerService>();

            services.AddSingleton<IMasterServerSessionService, MasterServerSessionService>();
            services.AddSingleton<MasterServerMessageSource>();
            services.AddSingleton<MasterServerMessageDispatcher>();

            services.AddHostedService<Implementations.MasterServer>();
            services.AddHostedService<MasterServerSessionTickService>();

            services.AddHostedService<HandshakeMessageHandler>();
            services.AddHostedService<UserMessageHandler>();
            services.AddHostedService<DedicatedServerMessageHandler>();
        }
    }
}
