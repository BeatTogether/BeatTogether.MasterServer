using System.Net;

namespace BeatTogether.MasterServer.Domain.Models
{
    public class Node
    {
        public IPAddress endpoint { get; }
        public bool Online { get; set; }
        public bool RemovedServerInstances { get; set; }

        public Node(IPAddress endPoint)
        {
            endpoint = endPoint;
            Online = true;
            RemovedServerInstances = false;
        }
    }
}
