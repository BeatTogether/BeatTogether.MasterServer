using System;
using System.IO;
using BeatTogether.MasterServer.Kernel.Abstractions.Security;
using BeatTogether.MasterServer.Kernel.Configuration;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace BeatTogether.MasterServer.Kernel.Implementations.Security
{
    public class CertificateSigningService : ICertificateSigningService
    {
        private readonly RsaPrivateCrtKeyParameters _privateKey;

        public CertificateSigningService(MasterServerConfiguration configuration)
        {
            using var streamReader = File.OpenText(configuration.PrivateKeyPath);
            var pemReader = new PemReader(streamReader);
            var @object = pemReader.ReadObject();
            var asymmetricCipherKeyPair = @object as AsymmetricCipherKeyPair;
            if (asymmetricCipherKeyPair != null)
                @object = asymmetricCipherKeyPair.Private;
            _privateKey = @object as RsaPrivateCrtKeyParameters;
            if (_privateKey is null)
                throw new Exception($"Invalid RSA private key (Path='{configuration.PrivateKeyPath}').");
        }

        public byte[] Sign(byte[] data)
        {
            var signer = SignerUtilities.GetSigner("SHA256WITHRSA");
            signer.Init(true, _privateKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }
    }
}
