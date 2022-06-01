using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Responses
{

    public record PublicServerSecretListFromDedicatedResponse(string[] Secrets);

    public record PublicServerSecretListResponse(string[] Secrets);

    public record PublicServerListResponse(SimpleServer[] Servers);

    public record ServerSecretListResponse(string[] Secrets);

    public record ServerListResponse(SimpleServer[] Servers);

    public record PublicServerCountResponse(int Servers);

    public record ServerCountResponse(int Severs);
}
