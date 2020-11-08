using BeatTogether.MasterServer.Kernel.Models;
using Org.BouncyCastle.Crypto.Parameters;

namespace BeatTogether.MasterServer.Kernel.Abstractions.Security
{
    public interface IDiffieHellmanService
    {
        ECKeyPair GetECKeyPair();
        ECPublicKeyParameters DeserializeECPublicKey(byte[] publicKey);
        byte[] GetPreMasterSecret(
            ECPublicKeyParameters publicKeyParameters,
            ECPrivateKeyParameters privateKeyParameters);
    }
}
