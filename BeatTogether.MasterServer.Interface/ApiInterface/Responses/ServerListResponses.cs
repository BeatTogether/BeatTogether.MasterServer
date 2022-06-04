using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Responses
{
    public record PublicServerSecretListResponse(string[] Secrets)
    {
        public bool Success => Secrets != null;
    }

    public record PublicServerListResponse(SimpleServer[] Servers)
    {
        public bool Success => Servers != null;
    }

    public record ServerSecretListResponse(string[] Secrets)
    {
        public bool Success => Secrets != null;
    }

    public record ServerListResponse(SimpleServer[] Servers)
    {
        public bool Success => Servers != null;
    }

    public record PublicServerCountResponse(int Servers)
    {
        public bool Success => Servers >= 0;
    }

    public record ServerCountResponse(int Servers)
    {
        public bool Success => Servers >= 0;
    }
}
