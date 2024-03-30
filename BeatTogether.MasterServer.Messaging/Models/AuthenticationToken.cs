using System;
using System.Text;
using BeatTogether.MasterServer.Messaging.Enums;

namespace BeatTogether.MasterServer.Messaging.Models
{
    public sealed class AuthenticationToken
    {
        public Platform Platform { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public byte[] SessionToken { get; set; }

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