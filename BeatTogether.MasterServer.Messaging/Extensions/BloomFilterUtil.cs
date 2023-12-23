using BeatTogether.MasterServer.Messaging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatTogether.MasterServer.Messaging.Extensions
{
    public static class BloomFilterUtil
    {
        public static T ToBloomFilter<T>(this string value, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>, new()
        {
            return new T().AddBloomFilterEntry(value, hashCount, hashBits);
        }

        public static T ToBloomFilter<T>(this IEnumerable<string> strings, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>, new()
        {
            return strings.Aggregate(new T(), (T bloomFilter, string str) => bloomFilter.AddBloomFilterEntry(str, hashCount, hashBits));
        }

        public static T ToBloomFilter<T>(this IEnumerable<uint> hashes, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>, new()
        {
            return hashes.Aggregate(new T(), (T bloomFilter, uint hash) => bloomFilter.AddBloomFilterEntryHash(hash, hashCount, hashBits));
        }

        public static T AddBloomFilterEntry<T>(this T bitMask, string value, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>
        {
            uint num = value.MurmurHash2();
            return bitMask.AddBloomFilterEntryHash(num, hashCount, hashBits);
        }

        public static T AddBloomFilterEntryHash<T>(this T bitMask, uint hash, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>
        {
            for (int i = 0; i < hashCount; i++)
            {
                bitMask = bitMask.SetBits((int)((ulong)hash % (ulong)((long)bitMask.bitCount)), 1UL);
                hash >>= hashBits;
            }
            return bitMask;
        }

        public static bool ContainsBloomFilterEntry<T>(this T bitMask, string value, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>
        {
            uint num = value.MurmurHash2();
            return bitMask.ContainsBloomFilterEntryHash(num, hashCount, hashBits);
        }

        public static bool ContainsBloomFilterEntryHash<T>(this T bitMask, uint hash, int hashCount = 3, int hashBits = 8) where T : IBitMask<T>
        {
            for (int i = 0; i < hashCount; i++)
            {
                if (bitMask.GetBits((int)((ulong)hash % (ulong)((long)bitMask.bitCount)), 1) == 0UL)
                {
                    return false;
                }
                hash >>= hashBits;
            }
            return true;
        }
    }

}
