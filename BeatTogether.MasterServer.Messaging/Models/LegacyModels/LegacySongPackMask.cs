using System;
using System.Collections.Generic;
using BeatTogether.Core.Messaging.Abstractions;
using BeatTogether.MasterServer.Messaging.Extensions;
using BeatTogether.MasterServer.Messaging.Models.JsonConverters;
using Krypton.Buffers;
using Newtonsoft.Json;

namespace BeatTogether.MasterServer.Messaging.Models.LegacyModels
{
    public sealed class LegacySongPackMask : IMessage
    {
        private const string StringPrefix = "[SongPackMask ";
        private const string StringSuffix = "]";

        private BitMask128 _bitMask;

        public ulong D0 => _bitMask.Top;
        public ulong D1 => _bitMask.Bottom;

        public LegacySongPackMask(string packId)
        {
            _bitMask = packId.ToBloomFilter<BitMask128>(2, 13);
        }

        public LegacySongPackMask(IEnumerable<string> packs)
        {
            _bitMask = packs.ToBloomFilter<BitMask128>(2, 13);
        }


        public LegacySongPackMask(BitMask128 bitMask = null)
        {
            _bitMask = bitMask ?? BitMask128.MinValue;
        }

        public LegacySongPackMask(ulong d0, ulong d1)
        {
            _bitMask = new BitMask128(d0, d1);
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

        public static bool TryParse(string str, out LegacySongPackMask result)
        {
            BitMask128 bloomFilter;

            if (BitMask128.TryParse(str, out bloomFilter))
            {
                result = new LegacySongPackMask(bloomFilter);
                return true;
            }

            if (str.StartsWith(StringPrefix) && str.EndsWith(StringSuffix) && BitMask128.TryParse(str,
                    StringPrefix.Length, str.Length - StringPrefix.Length - StringSuffix.Length, out bloomFilter))
            {
                result = new LegacySongPackMask(bloomFilter);
                return true;
            }

            result = default;
            return false;
        }

        public static LegacySongPackMask Parse(string str)
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