using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Models
{
    public record ECKeyPair
    {
        public ECPrivateKeyParameters PrivateKeyParameters;
        public byte[] PublicKey;
    }
}
