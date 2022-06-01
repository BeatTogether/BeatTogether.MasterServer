using BeatTogether.MasterServer.Interface.ApiInterface.Requests;
using BeatTogether.MasterServer.Interface.ApiInterface.Responses;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Abstractions
{
    public interface IApiInterface
    {
        Task<CreatedServerResponse> CreateServer(CreateServerRequest request);

        Task<RemoveSecretServerResponse> RemoveServer(RemoveSecretServerRequest request);

        Task<RemoveCodeServerResponse> RemoveServer(RemoveCodeServerCodeRequest request);

        Task<PublicServerSecretListFromDedicatedResponse> GetPublicServerSecrets(GetPrivateServerSecretsListFromDedicatedRequest request);

        Task<PublicServerSecretListResponse> GetPublicServerSecrets(GetPublicServerSecretsListRequest request);

        Task<ServerSecretListResponse> GetServerSecretsList(GetServerSecretsListRequest request);

        Task<PublicServerListResponse> GetPublicServers(GetPublicSimpleServersRequest request);

        Task<ServerListResponse> GetServers(GetSimpleServersRequest request);

        Task<PublicServerCountResponse> GetPublicServerCount(GetPublicServerCountRequest request);

        Task<ServerCountResponse> GetServerCount(GetServerCountRequest request);
    }
}
