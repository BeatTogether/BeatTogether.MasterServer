using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BeatTogether.Core.Security.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions;
using BeatTogether.MasterServer.Kernel.Abstractions.Providers;
using BeatTogether.MasterServer.Kernel.Abstractions.Sessions;
using BeatTogether.MasterServer.Kernel.Enums;
using BeatTogether.MasterServer.Messaging.Messages.Handshake;
using Krypton.Buffers;
using Serilog;

namespace BeatTogether.MasterServer.Kernel.Implementations
{
    public class HandshakeService : IHandshakeService
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly ICookieProvider _cookieProvider;
        private readonly IRandomProvider _randomProvider;
        private readonly X509Certificate2 _certificate;
        private readonly ICertificateSigningService _certificateSigningService;
        private readonly IDiffieHellmanService _diffieHellmanService;
        private readonly ILogger _logger;

        private static byte[] _masterSecretSeed = Encoding.UTF8.GetBytes("master secret");
        private static byte[] _keyExpansionSeed = Encoding.UTF8.GetBytes("key expansion");

        private const uint _epochMask = 0xff000000;

        public HandshakeService(
            IMessageDispatcher messageDispatcher,
            ICookieProvider cookieProvider,
            IRandomProvider randomProvider,
            X509Certificate2 certificate,
            ICertificateSigningService certificateSigningService,
            IDiffieHellmanService diffieHellmanService)
        {
            _messageDispatcher = messageDispatcher;
            _cookieProvider = cookieProvider;
            _randomProvider = randomProvider;
            _certificate = certificate;
            _certificateSigningService = certificateSigningService;
            _diffieHellmanService = diffieHellmanService;
            _logger = Log.ForContext<HandshakeService>();
        }

        #region Public Methods

        public Task<HelloVerifyRequest> ClientHello(ISession session, ClientHelloRequest request)
        {
            _logger.Verbose(
                $"Handling {nameof(ClientHelloRequest)} " +
                $"(Random='{BitConverter.ToString(request.Random)}')."
            );
            session.Epoch = request.RequestId & _epochMask;
            session.State = SessionState.New;
            session.Cookie = _cookieProvider.GetCookie();
            session.ClientRandom = request.Random;
            return Task.FromResult(new HelloVerifyRequest()
            {
                Cookie = session.Cookie
            });
        }

        public async Task<ServerHelloRequest> ClientHelloWithCookie(ISession session, ClientHelloWithCookieRequest request)
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
                    $"(EndPoint='{session.EndPoint}', " +
                    $"Cookie='{BitConverter.ToString(request.Cookie)}', " +
                    $"Expected='{BitConverter.ToString(session.Cookie ?? new byte[0])}')."
                );
                return null;
            }
            if (!request.Random.SequenceEqual(session.ClientRandom))
            {
                _logger.Warning(
                    $"Session sent {nameof(ClientHelloWithCookieRequest)} with a mismatching client random " +
                    $"(EndPoint='{session.EndPoint}', " +
                    $"Random='{BitConverter.ToString(request.Random)}', " +
                    $"Expected='{BitConverter.ToString(session.ClientRandom ?? new byte[0])}')."
                );
                return null;
            }

            // Generate a server random
            session.ServerRandom = _randomProvider.GetRandom();

            // Generate a key pair
            var keyPair = _diffieHellmanService.GetECKeyPair();
            session.ServerPrivateKeyParameters = keyPair.PrivateKeyParameters;

            // Generate a signature
            var signature = MakeSignature(session.ClientRandom, session.ServerRandom, keyPair.PublicKey);

            await _messageDispatcher.Send(session, new ServerCertificateRequest()
            {
                RequestId = session.GetNextRequestId(),
                ResponseId = request.CertificateResponseId,
                Certificates = new List<byte[]>() { _certificate.RawData }
            });
            return new ServerHelloRequest()
            {
                Random = session.ServerRandom,
                PublicKey = keyPair.PublicKey,
                Signature = signature
            };
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
            var masterSecretSeed = MakeSeed(_masterSecretSeed, session.ServerRandom, session.ClientRandom);
            var keyExpansionSeed = MakeSeed(_keyExpansionSeed, session.ServerRandom, session.ClientRandom);
            var sourceArray = PRF(
                PRF(session.PreMasterSecret, masterSecretSeed, 48),
                keyExpansionSeed,
                192
            );
            Array.Copy(sourceArray, 0, session.SendKey, 0, 32);
            Array.Copy(sourceArray, 32, session.ReceiveKey, 0, 32);
            Array.Copy(sourceArray, 64, sendMacSourceArray, 0, 64);
            Array.Copy(sourceArray, 128, receiveMacSourceArray, 0, 64);
            session.SendMac = new HMACSHA256(sendMacSourceArray);
            session.ReceiveMac = new HMACSHA256(receiveMacSourceArray);
            _logger.Information($"Session established (EndPoint='{session.EndPoint}').");
            return Task.FromResult(new ChangeCipherSpecRequest());
        }

        #endregion

        #region Private Methods

        private byte[] MakeSignature(byte[] clientRandom, byte[] serverRandom, byte[] publicKey)
        {
            var buffer = new GrowingSpanBuffer(stackalloc byte[512]);
            buffer.WriteBytes(clientRandom);
            buffer.WriteBytes(serverRandom);
            buffer.WriteBytes(publicKey);
            return _certificateSigningService.Sign(buffer.Data.ToArray());
        }

        private byte[] MakeSeed(byte[] baseSeed, byte[] serverSeed, byte[] clientSeed)
        {
            var seed = new byte[baseSeed.Length + serverSeed.Length + clientSeed.Length];
            Array.Copy(baseSeed, 0, seed, 0, baseSeed.Length);
            Array.Copy(serverSeed, 0, seed, baseSeed.Length, serverSeed.Length);
            Array.Copy(clientSeed, 0, seed, baseSeed.Length + serverSeed.Length, clientSeed.Length);
            return seed;
        }

        private byte[] PRF(byte[] key, byte[] seed, int length)
        {
            var i = 0;
            var array = new byte[length + seed.Length];
            while (i < length)
            {
                Array.Copy(seed, 0, array, i, seed.Length);
                PRFHash(key, array, ref i);
            }
            var array2 = new byte[length];
            Array.Copy(array, 0, array2, 0, length);
            return array2;
        }

        private void PRFHash(byte[] key, byte[] seed, ref int length)
        {
            using var hmacsha256 = new HMACSHA256(key);
            var array = hmacsha256.ComputeHash(seed, 0, length);
            var num = Math.Min(length + array.Length, seed.Length);
            Array.Copy(array, 0, seed, length, num - length);
            length = num;
        }

        #endregion
    }
}
