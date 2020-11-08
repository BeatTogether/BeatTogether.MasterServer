using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Implementations.Messages.Handshake;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeService : IHandshakeService
    {
        private readonly ICookieProvider _cookieProvider;
        private readonly IRandomProvider _randomProvider;
        private readonly ICertificateProvider _certificateProvider;
        private readonly ICertificateSigningService _certificateSigningService;
        private readonly IDiffieHellmanService _diffieHellmanService;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger _logger;

        private static byte[] _masterSecretSeed = Encoding.UTF8.GetBytes("master secret");
        private static byte[] _keyExpansionSeed = Encoding.UTF8.GetBytes("key expansion");

        public HandshakeService(
            ICookieProvider cookieProvider,
            IRandomProvider randomProvider,
            ICertificateProvider certificateProvider,
            ICertificateSigningService certificateSigningService,
            IDiffieHellmanService diffieHellmanService,
            ICryptoService cryptoService)
        {
            _cookieProvider = cookieProvider;
            _randomProvider = randomProvider;
            _certificateProvider = certificateProvider;
            _certificateSigningService = certificateSigningService;
            _diffieHellmanService = diffieHellmanService;
            _cryptoService = cryptoService;
            _logger = Log.ForContext<HandshakeService>();
        }

        public Task<HelloVerifyRequest> ClientHello(ISession session, ClientHelloRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloRequest)} " +
                $"(Random='{BitConverter.ToString(request.Random)}')."
            );
            session.State = SessionState.New;
            session.Cookie = _cookieProvider.GetCookie();
            session.ClientRandom = request.Random;
            return Task.FromResult(new HelloVerifyRequest()
            {
                Cookie = session.Cookie
            });
        }

        public Task<(ServerHelloRequest, ServerCertificateRequest)> ClientHelloWithCookie(ISession session, ClientHelloWithCookieRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloWithCookieRequest)} " +
                $"(CertificateResponseId={request.CertificateResponseId}, " +
                $"Random='{BitConverter.ToString(request.Random)}', " +
                $"Cookie='{BitConverter.ToString(request.Cookie)}')."
            );
            if (!request.Cookie.SequenceEqual(session.Cookie))
            {
                _logger.Warning(
                    $"Session sent {nameof(ClientHelloWithCookieRequest)} with a mismatching cookie " +
                    $"(Cookie='{BitConverter.ToString(request.Cookie)}', " +
                    $"Expected='{BitConverter.ToString(session.Cookie ?? new byte[0])}')."
                );
                return Task.FromResult<(ServerHelloRequest, ServerCertificateRequest)>((null, null));
            }
            if (!request.Random.SequenceEqual(session.ClientRandom))
            {
                _logger.Warning(
                    $"Session sent {nameof(ClientHelloWithCookieRequest)} with a mismatching client random " +
                    $"(Random='{BitConverter.ToString(request.Random)}', " +
                    $"Expected='{BitConverter.ToString(session.ClientRandom ?? new byte[0])}')."
                );
                return Task.FromResult<(ServerHelloRequest, ServerCertificateRequest)>((null, null));
            }

            // Generate a server random
            session.ServerRandom = _randomProvider.GetRandom();

            // Generate a key pair
            var keyPair = _diffieHellmanService.GetECKeyPair();
            session.ServerPrivateKeyParameters = keyPair.PrivateKeyParameters;

            // Generate a signature
            var certificate = _certificateProvider.GetCertificate();
            var buffer = new GrowingSpanBuffer(stackalloc byte[512]);
            buffer.WriteBytes(session.ClientRandom);
            buffer.WriteBytes(session.ServerRandom);
            buffer.WriteBytes(keyPair.PublicKey);
            var signature = _certificateSigningService.Sign(buffer.Data.ToArray());

            return Task.FromResult((
                new ServerHelloRequest()
                {
                    Random = session.ServerRandom,
                    PublicKey = keyPair.PublicKey,
                    Signature = signature
                },
                new ServerCertificateRequest()
                {
                    ResponseId = request.CertificateResponseId,
                    Certificates = new List<byte[]>() { certificate.RawData }
                }
            ));
        }

        public Task<ChangeCipherSpecRequest> ClientKeyExchange(ISession session, ClientKeyExchangeRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientKeyExchange)} " +
                $"(ClientPublicKey='{BitConverter.ToString(request.ClientPublicKey)}')."
            );
            session.ClientPublicKeyParameters = _diffieHellmanService.DeserializeECPublicKey(request.ClientPublicKey);
            session.PreMasterSecret = _diffieHellmanService.GetPreMasterSecret(
                session.ClientPublicKeyParameters,
                session.ServerPrivateKeyParameters
            );
            session.State = SessionState.Established;
            session.ReceiveKey = new byte[32];
            session.SendKey = new byte[32];
            var sendMacSourceArray = new byte[64];
            var receiveMacSourceArray = new byte[64];
            var masterSecretSeed = _cryptoService.MakeSeed(_masterSecretSeed, session.ServerRandom, session.ClientRandom);
            var keyExpansionSeed = _cryptoService.MakeSeed(_keyExpansionSeed, session.ServerRandom, session.ClientRandom);
            var sourceArray = _cryptoService.PRF(
                _cryptoService.PRF(session.PreMasterSecret, masterSecretSeed, 48),
                keyExpansionSeed,
                192
            );
            Array.Copy(sourceArray, 0, session.SendKey, 0, 32);
            Array.Copy(sourceArray, 32, session.ReceiveKey, 0, 32);
            Array.Copy(sourceArray, 64, sendMacSourceArray, 0, 64);
            Array.Copy(sourceArray, 128, receiveMacSourceArray, 0, 64);
            session.SendMac = new HMACSHA256(sendMacSourceArray);
            session.ReceiveMac = new HMACSHA256(receiveMacSourceArray);
            _logger.Information($"Session established (EndPoint={session.EndPoint}).");
            return Task.FromResult(new ChangeCipherSpecRequest());
        }
    }
}
