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

        private BitMask256 _bitMask;

        public ulong D0 => _bitMask.D0;
        public ulong D1 => _bitMask.D1;
        public ulong D2 => _bitMask.D2;
        public ulong D3 => _bitMask.D3;


        public SongPackMask(BitMask256 bitMask = null)
        {
            _bitMask = bitMask ?? BitMask256.MinValue;
        }

        public SongPackMask(ulong d0, ulong d1, ulong d2, ulong d3)
        {
            _bitMask = new BitMask256(d0, d1, d2, d3);
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
            BitMask256 bloomFilter;

            if (BitMask256.TryParse(str, out bloomFilter))
            {
                result = new SongPackMask(bloomFilter);
                return true;
            }

            if (str.StartsWith(StringPrefix) && str.EndsWith(StringSuffix) && BitMask256.TryParse(str,
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