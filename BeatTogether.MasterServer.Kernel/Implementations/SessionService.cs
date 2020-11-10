using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Implementations.MessageReceivers;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class SessionService : ISessionService
    {
        private readonly HandshakeMessageReceiver _handshakeMessageReceiver;
        private readonly UserMessageReceiver _userMessageReceiver;
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<EndPoint, ISession> _sessions;
        private readonly ConcurrentDictionary<EndPoint, CancellationTokenSource> _cancellationTokenSources;

        public SessionService(
            HandshakeMessageReceiver handshakeMessageReceiver,
            UserMessageReceiver userMessageReceiver)
        {
            _handshakeMessageReceiver = handshakeMessageReceiver;
            _userMessageReceiver = userMessageReceiver;
            _logger = Log.ForContext<SessionService>();

            _sessions = new ConcurrentDictionary<EndPoint, ISession>();
            _cancellationTokenSources = new ConcurrentDictionary<EndPoint, CancellationTokenSource>();
        }

        #region Public Methods

        public ISession OpenSession(MasterServer masterServer, EndPoint endPoint)
        {
            bool isNewSession = false;
            var session = _sessions.GetOrAdd(endPoint, key =>
            {
                isNewSession = true;
                return new Session(masterServer, key);
            });
            if (!isNewSession)
                return session;

            _logger.Information($"Opening session (EndPoint='{session.EndPoint}').");
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSources[endPoint] = new CancellationTokenSource();
            Task.Run(() => ReadMessageReceiveChannel(session, cancellationTokenSource.Token))
                .ConfigureAwait(false);

            return session;
        }

        public bool CloseSession(ISession session)
        {
            if (!_sessions.TryRemove(session.EndPoint, out _))
                return false;

            _logger.Information($"Closing session (EndPoint='{session.EndPoint}').");
            if (_cancellationTokenSources.TryRemove(session.EndPoint, out var cancellationTokenSource))
                cancellationTokenSource.Cancel();

            return true;
        }

        public ISession GetSession(EndPoint endPoint)
            => _sessions[endPoint];

        public bool TryGetSession(EndPoint endPoint, out ISession session)
            => _sessions.TryGetValue(endPoint, out session);

        #endregion

        #region Private Methods

        private async Task ReadMessageReceiveChannel(ISession session, CancellationToken cancellationToken)
        {
            while (await session.MessageReceiveChannel.Reader.WaitToReadAsync(cancellationToken))
                while (session.MessageReceiveChannel.Reader.TryRead(out IMessage message))
                {
                    try
                    {
                        // TODO: This logic should probably be expanded in case of other
                        // message receivers being added (i.e. dedicated servers)
                        if (message is not IEncryptedMessage)
                            await _handshakeMessageReceiver.OnReceived(session, message);
                        else
                            await _userMessageReceiver.OnReceived(session, message);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Exception thrown during message handler.");
                    }
                }
        }

        #endregion
    }
}
