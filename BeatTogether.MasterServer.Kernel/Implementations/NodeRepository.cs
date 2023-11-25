using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Autobus;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.Kernal.Abstractions;
using BeatTogether.MasterServer.Messaging.Enums;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class NodeRepository : INodeRepository
    //Stores the currently active nodes and whether they are online or not
    {
        private readonly ConcurrentDictionary<IPAddress, Node> _nodes = new();
        private bool WaitingForResponses;
        private readonly CheckNodesEvent checkNodes;

        private readonly int EndpointRecieveTimeout = 6000;

        private readonly IServerRepository _serverRepository;
        private readonly IAutobus _autobus;
        private readonly ILogger _logger = Log.ForContext<NodeRepository>();

        public NodeRepository(IServerRepository serverRepository, IAutobus autobus)
        {
            _serverRepository = serverRepository;
            _autobus = autobus;
        }

        public void StartWaitForAllNodesTask()
        {
            if (!WaitingForResponses)
            {
                WaitingForResponses = true;
                _autobus.Publish(checkNodes);
                _ = Task.Run(() => AsyncStartWaitForAllNodesTask());
            }
        }

        private async void AsyncStartWaitForAllNodesTask()
        {
            await Task.Delay(EndpointRecieveTimeout);

            foreach (var node in _nodes)
            {
                if (node.Value.Online && (DateTime.UtcNow - node.Value.LastOnline).TotalSeconds > 20) //10 seconds is the delay before StartWaitForAllNodesTask is called again, check its missed two pings
                {
                    _logger.Error("SERVER NODE IS OFFLINE or has not responded: " + node.Key);

                    await SetNodeOffline(node.Key);
                }
            }
            WaitingForResponses = false;
        }

        public ConcurrentDictionary<IPAddress, Node> GetNodes()
        {
            return _nodes;
        }

        public async Task SetNodeOnline(IPAddress endPoint, string Version)
        {
            _logger.Information($"Node is online: " + endPoint);

            if (!_nodes.TryAdd(endPoint, new Node(endPoint, Version)))
            {
                _logger.Information($"Resetting restarted node: " + endPoint);
                await SetNodeOffline(endPoint);
                _nodes[endPoint].NodeVersion = Version;
                _nodes[endPoint].LastStart = DateTime.UtcNow;
                _nodes[endPoint].LastOnline = DateTime.UtcNow;
                _nodes[endPoint].Online = true;
            }
            AwaitNodeResponses.TryAdd(endPoint, new());
        }
        public async Task SetNodeOffline(IPAddress endPoint)
        { 
            if (_nodes.ContainsKey(endPoint))
            {
                await _serverRepository.RemoveServersWithEndpoint(endPoint);
                _logger.Information("Removed servers that are on node " + endPoint + " from master repository");
                _nodes[endPoint].Online = false;
            }
        }

        public void ReceivedOK(IPAddress endPoint)
        {
            if (!WaitingForResponses)
                return;
            if (_nodes.TryGetValue(endPoint, out var node))
            {
                node.LastOnline = DateTime.UtcNow;
                node.Online = true;
            }
        }

        public bool EndpointExists(IPEndPoint endPoint)
            => _nodes.TryGetValue(endPoint.Address, out var node) && node.Online;

        public async Task<bool> DisconnectNode(IPAddress endPoint)
        {
            _logger.Information("Disconnecting and shutting down node: " + endPoint);
            await SetNodeOffline(endPoint);
            _nodes.TryRemove(endPoint, out _);
            _autobus.Publish(new ShutdownNodeEvent(endPoint.ToString())); //TODO add logic dedi side to shutdown said node
            return true;
        }



        readonly ConcurrentDictionary<IPAddress, ConcurrentDictionary<EndPoint, TaskCompletionSource<bool>>> AwaitNodeResponses = new();

        public async Task<bool> SendAndAwaitPlayerEncryptionRecievedFromNode(IPEndPoint NodeEndPoint,
            EndPoint SessionEndPoint, string UserId, string UserName, Platform platform, byte[] Random, 
            byte[] PublicKey, string PlayerSessionId, string Secret, int TimeOut)
        {
            if (!EndpointExists(NodeEndPoint))
                return false;
            if (!AwaitNodeResponses.TryGetValue(NodeEndPoint.Address, out var NodeResponses))//This happens if the node has gone offline
                return false;

            var task = new TaskCompletionSource<bool>();
            var EndpointsTimeout = new CancellationTokenSource();
            EndpointsTimeout.Token.Register(() => {
                task.TrySetResult(false);
            });

            if (!NodeResponses.TryAdd(SessionEndPoint, task))
            {
                NodeResponses[SessionEndPoint].SetResult(false);
                return false;
            }

            _autobus.Publish(new PlayerConnectedToMatchmakingServerEvent(
                NodeEndPoint.Address.ToString(),
                SessionEndPoint.ToString(),
                Random ?? Array.Empty<byte>(),
                PublicKey ?? Array.Empty<byte>(),
                PlayerSessionId ?? "",
                Secret
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

        public Node GetNode(string EndPoint)
        {
            
            if(_nodes.TryGetValue(IPEndPoint.Parse(EndPoint).Address, out var Node))
                return Node;
            return null;
        }
    }
}
