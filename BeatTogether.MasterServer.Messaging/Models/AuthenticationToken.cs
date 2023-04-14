using System;
using System.Text;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.Extensions;
using BeatTogether.MasterServer.Messaging.Enums;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class AuthenticationToken : IMessage
    {
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] SessionToken { get; set; }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            bufferWriter.WriteUInt8((byte) Platform);
            bufferWriter.WriteString(UserId);
            bufferWriter.WriteString(UserName);
            bufferWriter.WriteVarBytes(SessionToken);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            Platform = (Platform) bufferReader.ReadByte();
            UserId = bufferReader.ReadString();
            UserName = bufferReader.ReadString();
            SessionToken = bufferReader.ReadVarBytes().ToArray();
        }

        public static byte[] SessionTokenFromHex(string str)
        {
            var array = new byte[str.Length / 2];
            var i = 0;
            var num = 0;
            var num2 = 1;

            while (i < array.Length)
            {
                array[i] = (byte) (GetHexVal(str[num]) << 4 | GetHexVal(str[num2]));
                i++;
                num += 2;
                num2 += 2;
            }

            return array;
        }

        public static byte GetHexVal(char c)
        {
            if (c >= '0' && c <= '9')
                return (byte) (c - '0');

            if (c >= 'a' && c <= 'f')
                return (byte) ('\n' + c - 'a');

            if (c >= 'A' && c <= 'F')
                return (byte) ('\n' + c - 'A');

            throw new Exception($"Invalid Hex Char {c}");
        }
        
        public static byte[] SessionTokenFromUtf8(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }
}