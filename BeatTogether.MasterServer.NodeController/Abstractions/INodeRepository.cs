using System.Collections.Concurrent;
using System.Net;
using BeatTogether.Core.Abstractions;
using BeatTogether.MasterServer.Domain.Models;

namespace BeatTogether.MasterServer.NodeController.Abstractions
{
    public interface INodeRepository
    {
        public ConcurrentDictionary<IPAddress, Node> GetNodes();
        public Node? GetNode(string EndPoint);
        public Task SetNodeOnline(IPAddress endPoint, string Version);
        public Task SetNodeOffline(IPAddress endPoint);

        public void ReceivedOK(IPAddress endPoint);

        public void StartWaitForAllNodesTask();
        public bool EndpointExists(IPEndPoint endPoint);

        Task<bool> SendAndAwaitPlayerSessionDataRecievedFromNode(IPEndPoint NodeEndPoint, string InstanceSecret, IPlayer playerSessionData, int TimeOut);

        void OnNodeRecievedSessionDataParameters(IPEndPoint NodeEndPoint, string PlayerSessionId);
    }
}
