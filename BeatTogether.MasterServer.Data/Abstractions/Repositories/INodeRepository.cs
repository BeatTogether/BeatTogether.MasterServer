
using BeatTogether.MasterServer.Domain.Models;
using System.Collections.Concurrent;
using System.Net;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public interface INodeRepository
    {
        bool WaitingForResponses { get; set; }
        public ConcurrentDictionary<IPAddress, Node> GetNodes();
        public void SetNodeOnline(IPAddress endPoint);
        public void SetNodeOffline(IPAddress endPoint);

        public void ReceivedOK(IPAddress endPoint);

        public void StartWaitForAllNodesTask();
        public bool EndpointExists(IPEndPoint endPoint);
    }
}
