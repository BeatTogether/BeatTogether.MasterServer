using BeatTogether.MasterServer.Interface.ApiInterface.Models;

namespace BeatTogether.MasterServer.Interface.ApiInterface.Responses
{
    public record ServerJoinsCountResponse(long Joins);

    public record ServerListResponse(SimpleServer[] Servers)
    {
        public bool Success => Servers != null;
    }

    public record GetServerNodesResponse(ServerNode[] Nodes);
}
