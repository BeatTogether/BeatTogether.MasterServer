using BeatTogether.MasterServer.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Data.Abstractions.Repositories
{
    public class NodeRepository : INodeRepository
        //Stores the currently active nodes and whether they are online or not
    {
        private ConcurrentDictionary<IPAddress, Node> _nodes = new();
        public bool WaitingForResponses { get; set; }

        private ConcurrentDictionary<IPAddress, bool> ReceivedOk = new();
        private readonly ConcurrentDictionary<IPAddress, TaskCompletionSource> _EndpointsReceived = new();

        private readonly int EndpointRecieveTimeout = 4000;

        public void StartWaitForAllNodesTask()
        {
            foreach (var node in _nodes)
            {
                ReceivedOk.TryAdd(node.Key, false);
            }
            var EndpointsTimeout = new CancellationTokenSource();
            var LinkedTask = CancellationTokenSource.CreateLinkedTokenSource(EndpointsTimeout.Token);
            IEnumerable<Task> sceneReadyTasks = _nodes.Values.Select(p => {
                var task = _EndpointsReceived.GetOrAdd(p.endpoint, _ => new());
                LinkedTask.Token.Register(() => task.TrySetResult());
                return task.Task;
            });
            _ = Task.Run(()=> AsyncStartWaitForAllNodesTask(EndpointsTimeout, sceneReadyTasks));
        }

        private async void AsyncStartWaitForAllNodesTask(CancellationTokenSource EndpointsTimeout, IEnumerable<Task> sceneReadyTasks)
        {
            EndpointsTimeout.CancelAfter(EndpointRecieveTimeout);
            await Task.WhenAll(sceneReadyTasks);

            foreach (var node in ReceivedOk)
            {
                if (!node.Value)
                    SetNodeOffline(node.Key);
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
            if (!_nodes.ContainsKey(endPoint))
                _nodes.TryAdd(endPoint, new Node(endPoint));
            _nodes[endPoint].Online = true;
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
    }
}
