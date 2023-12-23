using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Extensions;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models.LegacyModels
{
    public class BitMask128 : IMessage, IBitMask<BitMask128>
    {
        public ulong Top { get; set; }
        public ulong Bottom { get; set; }

        public const int BitCount = 128;

        public int bitCount { get => BitCount; }

        public static BitMask128 MaxValue => new(ulong.MaxValue, ulong.MaxValue);
        public static BitMask128 MinValue => new(0U, 0U);

        public BitMask128()
        {
        }

        public BitMask128(ulong top = 0U, ulong bottom = 0U)
        {
            Top = top;
            Bottom = bottom;
        }

        #region Bits

        public BitMask128 SetBits(int offset, ulong bits)
        {
            return new BitMask128(Top | bits.ShiftLeft(offset - 64), Bottom | bits.ShiftLeft(offset));
        }

        public ulong GetBits(int offset, int count)
        {
            var num = (1UL << count) - 1UL;
            var num2 = offset - 64;

            return (Top.ShiftRight(num2) | Bottom.ShiftRight(offset)) & num;
        }

        #endregion

        #region Network

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt64(Top);
            bufferWriter.WriteUInt64(Bottom);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Top = bufferReader.ReadUInt64();
            Bottom = bufferReader.ReadUInt64();
        }

        #endregion

        #region Parse

        public static bool TryParse(string str, out BitMask128 bitMask)
            => TryParse(str, 0, str.Length, out bitMask);

        public static bool TryParse(string str, int offset, int length, out BitMask128 bitMask)
        {
            bitMask = new BitMask128();

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
            return Top.ToString("x16") + Bottom.ToString("x16");
        }

        #endregion

        #region Util

        private static char GetBase64Char(ulong digit)
        {
            if (digit < 26UL)
                return (char)(65UL + digit);
            if (digit < 52UL)
                return (char)(97UL + digit - 26UL);
            if (digit < 62UL)
                return (char)(48UL + digit - 52UL);
            if (digit != 62UL)
                return '/';
            return '+';
        }

        private static uint GetBase64Digit(char c)
        {
            if (c >= 'A' && c <= 'Z')
                return (uint)(c - 'A');
            if (c >= 'a' && c <= 'z')
                return (uint)('\u001a' + (c - 'a'));
            if (c >= '0' && c <= '9')
                return (uint)('4' + (c - '0'));
            if (c == '+')
                return 62U;
            if (c == '/')
                return 63U;
            return uint.MaxValue;
        }

        private static uint GetHexDigit(char c)
        {
            if (c >= '0' && c <= '9')
                return (uint)(c - '0');
            if (c >= 'a' && c <= 'f')
                return (uint)('\n' + (c - 'a'));
            if (c >= 'A' && c <= 'F')
                return (uint)('\n' + (c - 'A'));
            return uint.MaxValue;
        }

        #endregion
    }
}