using System;
using System.Security.Cryptography;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Models;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeService : IHandshakeService
    {
        private readonly RNGCryptoServiceProvider _rngCryptoServiceProvider;
        private readonly ILogger _logger;

        public HandshakeService(RNGCryptoServiceProvider rngCryptoServiceProvider)
        {
            _rngCryptoServiceProvider = rngCryptoServiceProvider;
            _logger = Log.ForContext<HandshakeService>();
        }

        public HelloVerifyRequest ClientHello(Session session, ClientHelloRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloRequest)} " +
                $"(Random='{BitConverter.ToString(request.Random).Replace("-", "")}')."
            );
            Span<byte> cookie = stackalloc byte[32];
            _rngCryptoServiceProvider.GetBytes(cookie);
            return new HelloVerifyRequest()
            {
                Cookie = cookie.ToArray()
            };
        }

        public (ServerHelloRequest, ServerCertificateRequest) ClientHelloWithCookie(Session session, ClientHelloWithCookieRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloWithCookieRequest)} " +
                $"(CertificateResponseId={request.CertificateResponseId}, " +
                $"Random='{BitConverter.ToString(request.Random).Replace("-", "")}', " +
                $"Cookie='{BitConverter.ToString(request.Cookie).Replace("-", "")}')."
            );
            return (null, null);
        }

        public ChangeCipherSpecRequest ClientKeyExchange(Session session, ClientKeyExchangeRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientKeyExchange)} " +
                $"(ClientPublicKey='{BitConverter.ToString(request.ClientPublicKey).Replace("-", "")}')."
            );
            return new ChangeCipherSpecRequest();
        }
    }
}
