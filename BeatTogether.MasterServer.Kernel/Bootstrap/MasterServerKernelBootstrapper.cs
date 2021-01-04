using System;
using System.Security.Cryptography;
using BeatTogether.Core.Hosting.Extensions;
using BeatTogether.Core.Messaging.Configuration;
using BeatTogether.Core.Security.Bootstrap;
using BeatTogether.DedicatedServer.Messaging.Abstractions;
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
using Obvs;
using Obvs.Configuration;
using Obvs.RabbitMQ.Configuration;
using Obvs.Serialization.Json.Configuration;
using Org.BouncyCastle.Security;
using Serilog;

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

            services.AddSingleton(serviceProvider =>
            {
                var rabbitMQConfiguration = serviceProvider.GetRequiredService<RabbitMQConfiguration>();
                Log.Information($"Building service bus (EndPoint='{rabbitMQConfiguration.EndPoint}').");
                var serviceBus = ServiceBus.Configure()
                    .WithRabbitMQEndpoints<IDedicatedServerMessage>()
                        .Named("DedicatedServer")
                        .ConnectToBroker(rabbitMQConfiguration.EndPoint)
                        .SerializedAsJson()
                        .AsClient()
                    .Create();
                serviceBus.Exceptions.Subscribe(e => Log.Error(e, $"Handling service bus exception."));
                return serviceBus;
            });

            services.AddTransient<SecureRandom>();
            services.AddTransient<RNGCryptoServiceProvider>();

            services.AddSingleton<ICookieProvider, CookieProvider>();
            services.AddSingleton<IRandomProvider, RandomProvider>();
            services.AddSingleton<IServerCodeProvider, ServerCodeProvider>();

            services.AddScoped<IHandshakeService, HandshakeService>();
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<IMasterServerSessionService, MasterServerSessionService>();
            services.AddSingleton<MasterServerMessageSource>();
            services.AddSingleton<MasterServerMessageDispatcher>();

            services.AddHostedService<Implementations.MasterServer>();
            services.AddHostedService<MasterServerSessionTickService>();

            services.AddHostedService<HandshakeMessageHandler>();
            services.AddHostedService<UserMessageHandler>();
        }
    }
}
