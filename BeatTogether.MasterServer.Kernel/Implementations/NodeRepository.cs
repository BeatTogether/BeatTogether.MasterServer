using Autobus;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernal.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class NodeRepository : INodeRepository
        //Stores the currently active nodes and whether they are online or not
    {
        private ConcurrentDictionary<IPAddress, Node> _nodes = new();
        public bool WaitingForResponses { get; set; }

        private ConcurrentDictionary<IPAddress, bool> ReceivedOk = new();
        private readonly ConcurrentDictionary<IPAddress, TaskCompletionSource> _EndpointsReceived = new();

        private readonly int EndpointRecieveTimeout = 4000;

        private readonly IServerRepository _serverRepository;
        private readonly IAutobus _autobus;

        public NodeRepository(IServerRepository serverRepository, IAutobus autobus)
        {
            _serverRepository = serverRepository;
            _autobus = autobus;
        }

        public void StartWaitForAllNodesTask()
        {
            foreach (var node in _nodes)
            {
                ReceivedOk.TryAdd(node.Key, false);
            }
            var EndpointsTimeout = new CancellationTokenSource();
            var LinkedTask = CancellationTokenSource.CreateLinkedTokenSource(EndpointsTimeout.Token);
            IEnumerable<Task> ServerRecieveTasks = _nodes.Values.Select(p => {
                var task = _EndpointsReceived.GetOrAdd(p.endpoint, _ => new());
                LinkedTask.Token.Register(() => task.TrySetResult());
                return task.Task;
            });
            _ = Task.Run(()=> AsyncStartWaitForAllNodesTask(EndpointsTimeout, ServerRecieveTasks));
        }

        private async void AsyncStartWaitForAllNodesTask(CancellationTokenSource EndpointsTimeout, IEnumerable<Task> ServerRecieveTasks)
        {
            EndpointsTimeout.CancelAfter(EndpointRecieveTimeout);
            await Task.WhenAll(ServerRecieveTasks);

            foreach (var node in ReceivedOk)
            {
                if (!node.Value)
                {
                    SetNodeOffline(node.Key);
                    if (!_nodes[node.Key].RemovedServerInstances)
                    {
                        await _serverRepository.RemoveServersWithEndpoint(node.Key);
                        _nodes[node.Key].RemovedServerInstances = true;
                    }
                }
            }
            WaitingForResponses = false;
            _EndpointsReceived.Clear();
            ReceivedOk.Clear();
        }

        public ConcurrentDictionary<IPAddress, Node> GetNodes()
        {
            return _nodes;
        }

        public void SetNodeOnline(IPAddress endPoint)
        {
            if (_nodes.ContainsKey(endPoint))
                _serverRepository.RemoveServersWithEndpoint(endPoint);
            else
            {
                _nodes.TryAdd(endPoint, new Node(endPoint));
                AwaitNodeResponses.TryAdd(endPoint, new());
            }
            _nodes[endPoint].Online = true;
            _nodes[endPoint].RemovedServerInstances = false;
        }
        public void SetNodeOffline(IPAddress endPoint)
        { 
            if (!_nodes.ContainsKey(endPoint))
                _nodes[endPoint].Online = false;
        }

        public void ReceivedOK(IPAddress endPoint)
        {
            if (!WaitingForResponses)
                return;
            ReceivedOk[endPoint] = true;
            if (_EndpointsReceived.TryGetValue(endPoint, out var tcs) && !tcs.Task.IsCompleted)
                tcs.SetResult();
        }

        public bool EndpointExists(IPEndPoint endPoint)
        {
            bool found = false;
            foreach(var node in GetNodes())
            {
                if(endPoint.Address.ToString() == node.Key.ToString() && node.Value.Online)
                {
                    found = true; break;
                }
            }
            return found;
        }














        ConcurrentDictionary<IPAddress, ConcurrentDictionary<EndPoint, TaskCompletionSource<bool>>> AwaitNodeResponses = new();

        public async Task<bool> SendAndAwaitPlayerEncryptionRecievedFromNode(IPEndPoint NodeEndPoint,EndPoint SessionEndPoint, string UserId, string UserName, byte[] Random, byte[] PublicKey, int TimeOut)
        {
            if (!EndpointExists(NodeEndPoint))
                return false;
            if (!AwaitNodeResponses.TryGetValue(NodeEndPoint.Address, out var NodeResponses))
                return false;

            var task = new TaskCompletionSource<bool>();
            var EndpointsTimeout = new CancellationTokenSource();
            var LinkedTask = CancellationTokenSource.CreateLinkedTokenSource(EndpointsTimeout.Token);
            LinkedTask.Token.Register(() => task.TrySetResult(false));

            if (!NodeResponses.TryAdd(SessionEndPoint, task))
                return false;

            _autobus.Publish(new PlayerConnectedToMatchmakingServerEvent(NodeEndPoint.Address.ToString(),
                SessionEndPoint.ToString(), UserId, UserName,
                Random, PublicKey
            ));

            EndpointsTimeout.CancelAfter(TimeOut);
            bool PlayerAdded = await AwaitNodeResponses[NodeEndPoint.Address][SessionEndPoint].Task;
            AwaitNodeResponses[NodeEndPoint.Address].TryRemove(SessionEndPoint, out _);
            return PlayerAdded;
        }

        public void OnNodeRecievedEncryptionParameters(IPEndPoint NodeEndPoint, EndPoint PlayerEndpoint)
        {
            AwaitNodeResponses[NodeEndPoint.Address][PlayerEndpoint].SetResult(true);
        }

    }
}
