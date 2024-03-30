
namespace BeatTogether.MasterServer.Messaging.Extensions
{
    public static class MurmurHash
    {
        public static uint MurmurHash2(this string key)
        {
            uint num = (uint)key.Length;
            uint num2 = 33U ^ num;
            int num3 = 0;
            while (num >= 4U)
            {
                uint num4 = (uint)(((uint)key[num3 + 3] << 24) | ((uint)key[num3 + 2] << 16) | ((uint)key[num3 + 1] << 8) | key[num3]);
                num4 *= 1540483477U;
                num4 ^= num4 >> 24;
                num4 *= 1540483477U;
                num2 *= 1540483477U;
                num2 ^= num4;
                num3 += 4;
                num -= 4U;
            }
            switch (num)
            {
                case 1U:
                    num2 ^= (uint)key[num3];
                    num2 *= 1540483477U;
                    break;
                case 2U:
                    num2 ^= (uint)((uint)key[num3 + 1] << 8);
                    num2 ^= (uint)key[num3];
                    num2 *= 1540483477U;
                    break;
                case 3U:
                    num2 ^= (uint)((uint)key[num3 + 2] << 16);
                    num2 ^= (uint)((uint)key[num3 + 1] << 8);
                    num2 ^= (uint)key[num3];
                    num2 *= 1540483477U;
                    break;
            }
            num2 ^= num2 >> 13;
            num2 *= 1540483477U;
            return num2 ^ (num2 >> 15);
        }
    }
}
