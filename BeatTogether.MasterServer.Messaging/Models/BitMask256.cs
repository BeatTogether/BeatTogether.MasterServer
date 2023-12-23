using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public class BitMask256 : IMessage, IBitMask<BitMask256>
    {
        public ulong D0 { get; set; }
        public ulong D1 { get; set; }
        public ulong D2 { get; set; }
        public ulong D3 { get; set; }

        public const int BitCount = 256;

        public int bitCount { get => BitCount; }

        public static BitMask256 MaxValue => new(ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, ulong.MaxValue);
        public static BitMask256 MinValue => new();

        public BitMask256()
        {
        }

        public BitMask256(ulong d0 = 0U, ulong d1 = 0U, ulong d2 = 0U, ulong d3 = 0U)
        {
            D0 = d0;
            D1 = d1;
            D2 = d2;
            D3 = d3;
        }

        #region Bits

        public BitMask256 SetBits(int offset, ulong bits)
        {
            ulong d = D0;
            int num = offset - 192;
            ulong num2 = d | (bits).ShiftLeft(num);
            ulong d2 = D1;
            int num3 = offset - 128;
            ulong num4 = d2 | (bits).ShiftLeft(num3);
            ulong d3 = D2;
            int num5 = offset - 64;
            return new BitMask256(num2, num4, d3 | (bits).ShiftLeft(num5), D3 | (bits).ShiftLeft(offset));
        }

        public ulong GetBits(int offset, int count)
        {
            ulong num = (1UL << count) - 1UL;
            int num2 = offset - 192;
            ulong num3 = (D0).ShiftRight(num2);
            int num4 = offset - 128;
            ulong num5 = num3 | (D1).ShiftRight(num4);
            int num6 = offset - 64;
            return (num5 | (D2).ShiftRight(num6) | (D3).ShiftRight(offset)) & num;
        }

        #endregion

        #region Network

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt64(D0);
            bufferWriter.WriteUInt64(D1);
            bufferWriter.WriteUInt64(D2);
            bufferWriter.WriteUInt64(D3);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            D0 = bufferReader.ReadUInt64();
            D1 = bufferReader.ReadUInt64();
            D2 = bufferReader.ReadUInt64();
            D3 = bufferReader.ReadUInt64();
        }

        #endregion

        #region Parse

        public static bool TryParse(string str, out BitMask256 bitMask)
            => TryParse(str, 0, str.Length, out bitMask);

        public static bool TryParse(string str, int offset, int length, out BitMask256 bitMask)
        {
            bitMask = new BitMask256();

            if (length == BitCount / 4)
            {
                for (var i = 0; i < length; i++)
                {
                    uint hexDigit;
                    if ((hexDigit = GetHexDigit(str[offset + i])) == 4294967295U)
                        return false;
                    bitMask = bitMask.SetBits(BitCount - i * 4 - 4, hexDigit);
                }

                return true;
            }

            if (length == (BitCount + 5) / 6)
            {
                for (var j = 0; j < length; j++)
                {
                    uint base64Digit;
                    if ((base64Digit = GetBase64Digit(str[offset + j])) == 4294967295U)
                        return false;
                    bitMask = bitMask.SetBits(BitCount - j * 6 - 6, base64Digit);
                }

                return true;
            }

            return false;
        }

        #endregion

        #region ToString

        public string ToShortString()
        {
            var chars = new char[(BitCount + 5) / 6];

            for (var i = 0; i < chars.Length; i++)
                chars[i] = GetBase64Char(GetBits(BitCount - i * 6 - 6, 6));

            return new string(chars);
        }

        public override string ToString()
        {
            return D0.ToString("x16") + D1.ToString("x16") + D2.ToString("x16") + D3.ToString("x16");
        }

        #endregion

        #region Util

        private static char GetBase64Char(ulong digit)
        {
            if (digit < 26UL)
                return (char) (65UL + digit);
            if (digit < 52UL)
                return (char) (97UL + digit - 26UL);
            if (digit < 62UL)
                return (char) (48UL + digit - 52UL);
            if (digit != 62UL)
                return '/';
            return '+';
        }

        private static uint GetBase64Digit(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return (uint) (c - 'A');
            if (c >= 'a' && c <= 'z')
                return (uint) ('\u001a' + (c - 'a'));
            if (c >= '0' && c <= '9')
                return (uint) ('4' + (c - '0'));
            if (c == '+')
                return 62U;
            if (c == '/')
                return 63U;
            return uint.MaxValue;
        }

        private static uint GetHexDigit(char c)
        {
            if (c >= '0' && c <= '9')
                return (uint) (c - '0');
            if (c >= 'a' && c <= 'f')
                return (uint) ('\n' + (c - 'a'));
            if (c >= 'A' && c <= 'F')
                return (uint) ('\n' + (c - 'A'));
            return uint.MaxValue;
        }

        #endregion
    }
}