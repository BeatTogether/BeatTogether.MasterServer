using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;
using BeatTogether.MasterServer.Kernel.Configuration;
using BeatTogether.MasterServer.Kernel.Implementations;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Kernel.Implementations.Providers;
using BeatTogether.MasterServer.Kernel.Implementations.Security;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Security;

namespace BeatTogether.MasterServer.Kernel.Bootstrap
{
    public static class MasterServerKernelStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MasterServerConfiguration>();
            services.AddSingleton<MessagingConfiguration>();

            services.AddTransient<SecureRandom>();
            services.AddTransient<RNGCryptoServiceProvider>();

            services.AddSingleton<IRequestIdProvider, RequestIdProvider>();
            services.AddSingleton<ICookieProvider, CookieProvider>();
            services.AddSingleton<IRandomProvider, RandomProvider>();
            services.AddSingleton<ICertificateProvider, CertificateProvider>();
            services.AddSingleton<IServerCodeProvider, ServerCodeProvider>();

            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();

            services.AddSingleton<HandshakeMessageReceiver>();
            services.AddSingleton<UserMessageReceiver>();

            services.AddScoped<IHandshakeService, HandshakeService>();
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<IDiffieHellmanService, DiffieHellmanService>();
            services.AddSingleton<ICertificateSigningService, CertificateSigningService>();
            services.AddSingleton<ISessionService, SessionService>();
            services.AddSingleton<IMultipartMessageService, MultipartMessageService>();

            services.AddHostedService<Implementations.MasterServer>();
        }
    }
}
