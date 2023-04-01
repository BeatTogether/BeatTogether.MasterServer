using System;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Models.JsonConverters;
using Krypton.Buffers;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models
{
    [JsonConverter(typeof(SongPackMaskConverter))]
    public sealed class SongPackMask : IMessage
    {
        private const string StringPrefix = "[SongPackMask ";
        private const string StringSuffix = "]";

        private BitMask128 _bitMask;

        public ulong Top => _bitMask.Top;
        public ulong Bottom => _bitMask.Bottom;

        public SongPackMask(BitMask128 bitMask = null)
        {
            _bitMask = bitMask ?? BitMask128.MinValue;
        }

        public SongPackMask(ulong top, ulong bottom)
        {
            _bitMask = new BitMask128(top, bottom);
        }

        public void WriteTo(ref SpanBufferWriter bufferWriter)
        {
            _bitMask.WriteTo(ref bufferWriter);
        }

        public void ReadFrom(ref SpanBufferReader bufferReader)
        {
            _bitMask.ReadFrom(ref bufferReader);
        }

        #region String serialize

        public static bool TryParse(string str, out SongPackMask result)
        {
            BitMask128 bloomFilter;

            if (BitMask128.TryParse(str, out bloomFilter))
            {
                result = new SongPackMask(bloomFilter);
                return true;
            }

            if (str.StartsWith(StringPrefix) && str.EndsWith(StringSuffix) && BitMask128.TryParse(str,
                    StringPrefix.Length, str.Length - StringPrefix.Length - StringSuffix.Length, out bloomFilter))
            {
                result = new SongPackMask(bloomFilter);
                return true;
            }

            result = default(SongPackMask);
            return false;
        }

        public static SongPackMask Parse(string str)
        {
            if (TryParse(str, out var result))
                return result;

            throw new ArgumentException("Invalid SongPackMask: " + str);
        }

        public string ToShortString()
        {
            return _bitMask.ToShortString();
        }

        public override string ToString()
        {
            return _bitMask.ToString();
        }

        #endregion
    }
}