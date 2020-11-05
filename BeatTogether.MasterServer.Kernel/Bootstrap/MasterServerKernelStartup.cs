using System.Security.Cryptography;
using BeatTogether.MasterServer.Configuration;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace BeatTogether.MasterServer.Kernel.Bootstrap
{
    public static class MasterServerKernelStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<RNGCryptoServiceProvider>();

            services.AddSingleton<MasterServerConfiguration>();

            services.AddSingleton<IRequestIdProvider, RequestIdProvider>();
            services.AddSingleton<ICookieProvider, CookieProvider>();

            services.AddSingleton<HandshakeMessageReceiver>();
            services.AddSingleton<UserMessageReceiver>();

            services.AddScoped<IHandshakeService, HandshakeService>();
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<ISessionService, SessionService>();

            services.AddHostedService<Implementations.MasterServer>();
        }
    }
}
