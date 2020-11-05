using System;
using System.Collections.Generic;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Models;
using BeatTogether.MasterServer.Kernel.Static;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeService : IHandshakeService
    {
        private readonly ICookieProvider _cookieProvider;
        private readonly ILogger _logger;

        public HandshakeService(ICookieProvider cookieProvider)
        {
            _cookieProvider = cookieProvider;
            _logger = Log.ForContext<HandshakeService>();
        }

        public HelloVerifyRequest ClientHello(Session session, ClientHelloRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloRequest)} " +
                $"(Random='{BitConverter.ToString(request.Random)}')."
            );
            session.ClientRandom = request.Random;
            session.ServerRandom = _cookieProvider.GetCookie();
            return new HelloVerifyRequest()
            {
                Cookie = session.ServerRandom
            };
        }

        public (ServerHelloRequest, ServerCertificateRequest) ClientHelloWithCookie(Session session, ClientHelloWithCookieRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloWithCookieRequest)} " +
                $"(CertificateResponseId={request.CertificateResponseId}, " +
                $"Random='{BitConverter.ToString(request.Random)}', " +
                $"Cookie='{BitConverter.ToString(request.Cookie)}')."
            );
            if (!IsValidCookie(request.Cookie, session.ServerRandom))
            {
                _logger.Warning(
                    $"Session sent {nameof(ClientHelloWithCookieRequest)} with an invalid cookie " +
                    $"(Cookie='{BitConverter.ToString(request.Cookie)}', " +
                    $"Expected='{BitConverter.ToString(session.ServerRandom ?? new byte[0])}')."
                );
                return (null, null);
            }
            if (!IsValidCookie(request.Random, session.ClientRandom))
            {
                _logger.Warning(
                    $"Session sent {nameof(ClientHelloWithCookieRequest)} with an invalid random " +
                    $"(Random='{BitConverter.ToString(request.Random)}', " +
                    $"Expected='{BitConverter.ToString(session.ClientRandom ?? new byte[0])}')."
                );
                return (null, null);
            }

            // TODO: Calculate signature
            return (
                new ServerHelloRequest()
                {
                    Cookie = session.ServerRandom
                },
                new ServerCertificateRequest()
                {
                    ResponseId = request.ResponseId,
                    Certificates = new List<byte[]>()
                    {
                        new byte[0]
                    }
                }
            );
        }

        public ChangeCipherSpecRequest ClientKeyExchange(Session session, ClientKeyExchangeRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientKeyExchange)} " +
                $"(ClientPublicKey='{BitConverter.ToString(request.ClientPublicKey).Replace("-", "")}')."
            );
            return new ChangeCipherSpecRequest();
        }

        #region Private Methods

        public bool IsValidCookie(byte[] a, byte[] b)
        {
            if (a.Length != 32 || b.Length != 32)
                return false;

            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        #endregion
    }
}
