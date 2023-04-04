using System.Runtime.CompilerServices;

namespace BeatTogether.MasterServer.Messaging.Extensions
{
    public static class ULongExtensions
    {
        public static ulong ShiftLeft(this ulong value, in int shift)
        {
            if (shift < 0)
            {
                int num = -shift;
                return value.ShiftRight(num);
            }

            if (shift < 64)
            {
                return value << shift;
            }

            return 0UL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ShiftRight(this ulong value, in int shift)
        {
            if (shift < 0)
            {
                int num = -shift;
                return value.ShiftLeft(num);
            }

            if (shift < 64)
            {
                return value >> shift;
            }

            return 0UL;
        }
    }
}