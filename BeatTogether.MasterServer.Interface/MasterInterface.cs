﻿using Autobus;
using BeatTogether.MasterServer.Interface.Events;

namespace BeatTogether.MasterServer.Interface
{
    public class MasterInterfaceService
    {


        public class MasterServerServiceContract : BaseServiceContract
        {
            public override void Build(IServiceContractBuilder builder) =>
                builder
                    .UseName("MasterServer")
                    .AddEvent<CheckNodesEvent>()
                    .AddEvent<ShutdownNodeEvent>()
                    .AddEvent<DisconnectPlayerFromMatchmakingServerEvent>()
                    .AddEvent<PlayerSessionDataSendToDediEvent>()
                    .AddEvent<CloseServerInstanceEvent>();
        }
    }
}
