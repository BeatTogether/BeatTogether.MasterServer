namespace BeatTogether.MasterServer.Kernel.Abstractions.Security
{
    public interface ICryptoService
    {
        int IvLength { get; }

        byte[] GetIv();
        void Encrypt(byte[] buffer, byte[] key, byte[] iv);
        void Decrypt(byte[] buffer, byte[] key, byte[] iv);
        byte[] MakeSeed(byte[] baseSeed, byte[] serverSeed, byte[] clientSeed);
        byte[] PRF(byte[] key, byte[] seed, int length);
        void PRFHash(byte[] key, byte[] seed, ref int length);
    }
}
