namespace BeatTogether.MasterServer.Kernel.Abstractions.Security
{
    public interface ICertificateSigningService
    {
        byte[] Sign(byte[] data);
    }
}
