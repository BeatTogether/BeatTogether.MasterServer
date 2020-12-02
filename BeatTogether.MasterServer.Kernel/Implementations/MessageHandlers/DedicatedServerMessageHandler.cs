using System;
using BeatTogether.Core.Messaging.Implementations;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Messaging.Messages.DedicatedServer;

namespace BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers
{
    public class DedicatedServerMessageHandler : BaseMessageHandler<IDedicatedServerService>
    {
        public DedicatedServerMessageHandler(
            MasterServerMessageSource messageSource,
            MasterServerMessageDispatcher messageDispatcher,
            IServiceProvider serviceProvider)
            : base(messageSource, messageDispatcher, serviceProvider)
        {
            Register<AuthenticateDedicatedServerRequest, AuthenticateDedicatedServerResponse>(
                (service, session, request) => service.Authenticate(
                    (MasterServerSession)session, request
                )
            );
            Register<DedicatedServerNoLongerOccupiedRequest>(
                (service, session, request) => service.NoLongerOccupied(
                    (MasterServerSession)session, request
                )
            );
            Register<DedicatedServerHeartbeatRequest>(
                (service, session, request) => service.Heartbeat(
                    (MasterServerSession)session, request
                )
            );
            Register<RelayServerStatusUpdateRequest>(
                (service, session, request) => service.RelayServerStatusUpdate(
                    (MasterServerSession)session, request
                )
            );
            Register<MatchmakingServerStatusUpdateRequest>(
                (service, session, request) => service.MatchmakingServerStatusUpdate(
                    (MasterServerSession)session, request
                )
            );
            Register<DedicatedServerShutDownRequest>(
                (service, session, request) => service.ShutDown(
                    (MasterServerSession)session, request
                )
            );
        }
    }
}
