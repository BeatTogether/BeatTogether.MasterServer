using System.Collections.Concurrent;
using System.Net;
using Autobus;
using BeatTogether.MasterServer.Data.Abstractions.Repositories;
using BeatTogether.MasterServer.Domain.Models;
using BeatTogether.MasterServer.Interface.Events;
using BeatTogether.MasterServer.NodeController.Abstractions;
using Serilog;
using BeatTogether.Core.Abstractions;

namespace BeatTogether.MasterServer.NodeController.Implementations
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
        private readonly Serilog.ILogger _logger = Log.ForContext<NodeRepository>();

        public NodeRepository(IServerRepository serverRepository, IAutobus autobus)
        {
            _serverRepository = serverRepository;
            _autobus = autobus;
            checkNodes = new();
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
            await Task.Delay(EndpointRecieveTimeout); //Waits 4 seconds - is enough time for all nodes to send a response back to the master server
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
            var _version = new Version(Version);
            _logger.Information($"Node is online: " + endPoint + " Node version: " + _version.ToString());
            if (!_nodes.TryAdd(endPoint, new Node(endPoint, _version)))
            {
                _logger.Information($"Resetting restarted node: " + endPoint);
                await SetNodeOffline(endPoint); 
                _nodes[endPoint].NodeVersion = _version;
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



        readonly ConcurrentDictionary<IPAddress, ConcurrentDictionary<string, TaskCompletionSource<bool>>> AwaitNodeResponses = new();

        public async Task<bool> SendAndAwaitPlayerSessionDataRecievedFromNode(IPEndPoint NodeEndPoint, string ServerInstanceSecret, IPlayer playerSessionData, int TimeOut)
        {
            _logger.Information("StartingSend player session info");
            if (!EndpointExists(NodeEndPoint))
                return false;
            _logger.Information("Node exists");
            if (!AwaitNodeResponses.TryGetValue(NodeEndPoint.Address, out var NodeResponses))//This happens if the node has gone offline
                return false;
            _logger.Information("Node is still online/there is a node response in the dict for it");
            var task = new TaskCompletionSource<bool>();
            var EndpointsTimeout = new CancellationTokenSource();
            EndpointsTimeout.Token.Register(() => {
                _logger.Information("Timed out, setting recieved to false");
                task.TrySetResult(false);
            });

            if (!NodeResponses.TryAdd(playerSessionData.PlayerSessionId, task))
            {
                NodeResponses[playerSessionData.PlayerSessionId].SetResult(false);
                _logger.Information("Returning false as the session ID cannot be added to the await task list");
                return false;
            }
            _autobus.Publish(new PlayerSessionDataSendToDediEvent(NodeEndPoint.Address.ToString(), ServerInstanceSecret, new Core.ServerMessaging.Models.Player(playerSessionData)));
            _logger.Information("Published message for dedi to respond to, and now awaiting up to 2 sec for a response");
            EndpointsTimeout.CancelAfter(TimeOut);
            bool PlayerAdded = await AwaitNodeResponses[NodeEndPoint.Address][playerSessionData.PlayerSessionId].Task;
            AwaitNodeResponses[NodeEndPoint.Address].TryRemove(playerSessionData.PlayerSessionId, out _);
            _logger.Information("Removed player session from dict, and returning the value for their session data: " +  PlayerAdded);
            return PlayerAdded;
        }


        public void OnNodeRecievedSessionDataParameters(IPEndPoint NodeEndPoint, string playerSessionId)
        {
            if (AwaitNodeResponses[NodeEndPoint.Address].TryGetValue(playerSessionId, out var task))
                task.TrySetResult(true);
        }

        public Node? GetNode(string EndPoint)
        {
            if(_nodes.TryGetValue(IPEndPoint.Parse(EndPoint).Address, out var Node))
                return Node;
            return null;
        }
    }
}
