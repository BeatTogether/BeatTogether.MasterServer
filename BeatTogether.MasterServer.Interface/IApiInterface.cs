using Autobus;
using BeatTogether.MasterServer.Interface.ApiInterface.Requests;
using BeatTogether.MasterServer.Interface.ApiInterface.Responses;
using BeatTogether.MasterServer.Interface.Events;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Interface.ApiInterface
{
    public interface IApiInterface
    {
        Task<CreatedServerResponse> CreateServer(CreateServerRequest request); //TODO this causes a crash when called on the API server
        Task<RemoveSecretServerResponse> RemoveServer(RemoveSecretServerRequest request);
        Task<RemoveCodeServerResponse> RemoveServer(RemoveCodeServerCodeRequest request);
        Task<PublicServerSecretListFromDedicatedResponse> GetPublicServerSecrets(GetPrivateServerSecretsListFromDedicatedRequest request);
        Task<PublicServerSecretListResponse> GetPublicServerSecrets(GetPublicServerSecretsListRequest request);
        Task<ServerSecretListResponse> GetServerSecretsList(GetServerSecretsListRequest request);
        Task<PublicServerListResponse> GetPublicServers(GetPublicSimpleServersRequest request);
        Task<ServerListResponse> GetServers(GetSimpleServersRequest request);
        Task<PublicServerCountResponse> GetPublicServerCount(GetPublicServerCountRequest request);
        Task<ServerCountResponse> GetServerCount(GetServerCountRequest request);
        Task<ServerFromCodeResponse> GetServerFromCode(GetServerFromCodeRequest request);
        Task<ServerFromSecretResponse> GetServerFromSecret(GetServerFromSecretRequest request);

        public class MasterServerServiceContract : BaseServiceContract
        {
            public override void Build(IServiceContractBuilder builder) =>
                builder
                    .UseName("MasterServer")
                    .AddInterface<IApiInterface>()
                    .AddEvent<PlayerConnectedToMatchmakingServerEvent>();
        }
    }
}
