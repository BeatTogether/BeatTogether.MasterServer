﻿using System.Security.Cryptography;
using Autobus;
using BeatTogether.Core.Abstractions;
using BeatTogether.DedicatedServer.Interface;
using BeatTogether.MasterServer.Interface;
using BeatTogether.MasterServer.NodeController;
using BeatTogether.MasterServer.NodeController.Abstractions;
using BeatTogether.MasterServer.NodeController.Configuration;
using BeatTogether.MasterServer.NodeController.Implementations;
using BeatTogether.MasterServer.NodeController.Implementations.Sessions;


namespace BeatTogether.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseMasterServerNodeController(this IHostBuilder hostBuilder) =>
            hostBuilder
                .ConfigureAppConfiguration()
                .UseSerilog()
                .UseAutobus()
                .UseMasterServerData()
                .ConfigureServices((hostBuilderContext, services) =>
                    services
                        .AddSingleton<MasterInterfaceService>()
                        .AddConfiguration<NodeControllerConfiguration>("ServerConfiguration")
                        .AddSingleton(RandomNumberGenerator.Create())
                        .AddSingleton<INodeRepository, NodeRepository>()
                        .AddSingleton<ILayer2, NodeControllerLayer>()
                        .AddServiceClient<IMatchmakingService>()
                        .AddHostedService<DedicatedServerEventHandler>()
                        .AddHostedService<MasterServerSessionTickService>()
                        .AddOptions()
                );
    }
}
