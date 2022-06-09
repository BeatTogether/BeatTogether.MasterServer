
using BeatTogether.MasterServer.Domain.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Kernal.Abstractions
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

        Task<bool> SendAndAwaitPlayerEncryptionRecievedFromNode(IPEndPoint NodeEndPoint, EndPoint SessionEndPoint, string UserId, string UserName, byte[] Random, byte[] PublicKey, int TimeOut);
        void OnNodeRecievedEncryptionParameters(IPEndPoint NodeEndPoint, EndPoint PlayerEndpoint);
    }
}
