using Autobus;
using BeatTogether.MasterServer.Interface.ApiInterface.Requests;
using BeatTogether.MasterServer.Interface.ApiInterface.Responses;
using BeatTogether.MasterServer.Interface.Events;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Interface.ApiInterface
{
    public interface IApiInterface
    {
        Task<CreatedServerResponse> CreateServer(CreateServerRequest request);
        Task<ServerListResponse> GetServers(GetServersRequest request);
        Task<DisconnectPlayerResponse> KickPlayer(DisconnectPlayerRequest request);
        Task<RemoveServerResponse> StopServer(RemoveServerRequest request);
        Task<ServerJoinsCountResponse> GetPlayerJoins(GetPlayerJoins request);
        Task<GetServerNodesResponse> GetNodes(GetServerNodesRequest request);

        public class MasterServerServiceContract : BaseServiceContract
        {
            public override void Build(IServiceContractBuilder builder) =>
                builder
                    .UseName("MasterServer")
                    .AddInterface<IApiInterface>()
                    .AddEvent<CheckNodesEvent>()
                    .AddEvent<DisconnectPlayerFromMatchmakingServerEvent>()
                    .AddEvent<PlayerConnectedToMatchmakingServerEvent>()
                    .AddEvent<CloseServerInstanceEvent>();
        }
    }
}
