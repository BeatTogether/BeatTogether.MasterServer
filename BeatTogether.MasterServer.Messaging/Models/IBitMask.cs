namespace BeatTogether.MasterServer.Messaging.Models
{
    public interface IBitMask<T>
    {
        int bitCount { get; }

        T SetBits(int offset, ulong bits);

        ulong GetBits(int offset, int count);
    }
}
