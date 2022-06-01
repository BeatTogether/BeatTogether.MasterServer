using Autobus;
using BeatTogether.MasterServer.Interface.ApiInterface.Abstractions;
using BeatTogether.MasterServer.Interface.Events;

namespace BeatTogether.MasterServer.Interface
{
    public class MasterServerServiceContract : BaseServiceContract
    {
        public override void Build(IServiceContractBuilder builder) =>
            builder
                .UseName("MasterServer")
                .AddInterface<IApiInterface>()
                .AddEvent<PlayerConnectedToMatchmakingServerEvent>();
    }
}
