using System;
using System.Net;
using System.Text;
using Krypton.Buffers;

namespace BeatTogether.MasterServer.Messaging.Extensions
{
    public static class BufferExtensions
    {
        public static void WriteVarInt(this GrowingSpanBuffer buffer, int value)
            => buffer.WriteVarLong(value);

        public static int ReadVarInt(this SpanBufferReader bufferReader)
            => (int)bufferReader.ReadVarLong();

        public static void WriteVarUInt(this GrowingSpanBuffer buffer, uint value)
            => buffer.WriteVarULong(value);

        public static uint ReadVarUInt(this SpanBufferReader bufferReader)
            => (uint)bufferReader.ReadVarULong();

        public static void WriteVarLong(this GrowingSpanBuffer buffer, long value)
            => buffer.WriteVarULong((value < 0L ? (ulong)((-value << 1) - 1L) : (ulong)(value << 1)));

        public static long ReadVarLong(this SpanBufferReader bufferReader)
        {
            var varULong = (long)bufferReader.ReadVarULong();
            if ((varULong & 1L) != 1L)
                return varULong >> 1;
            return -(varULong >> 1) + 1L;
        }

        public static void WriteVarULong(this GrowingSpanBuffer buffer, ulong value)
        {
            do
            {
                var b = (byte)(value & 127UL);
                value >>= 7;
                if (value != 0UL)
                    b |= 128;
                buffer.WriteUInt8(b);
            } while (value != 0UL);
        }

        public static ulong ReadVarULong(this SpanBufferReader bufferReader)
        {
            ulong num = 0UL;
            int num2 = 0;
            ulong num3;
            while (((num3 = bufferReader.ReadUInt8()) & 128UL) != 0UL)
            {
                num |= (num3 & 127UL) << num2;
                num2 += 7;
            }
            return num | num3 << num2;
        }

        public static bool TryReadVarUInt(this SpanBufferReader bufferReader, out uint value)
        {
            ulong num;
            if (bufferReader.TryReadVarULong(out num) && (num >> 32) == 0UL)
            {
                value = (uint)num;
                return true;
            }
            value = 0U;
            return false;
        }

        public static bool TryReadVarULong(this SpanBufferReader bufferReader, out ulong value)
        {
            value = 0UL;
            int num = 0;
            while (num <= 63 && bufferReader.RemainingSize >= 1)
            {
                var b = bufferReader.ReadUInt8();
                value |= (ulong)(b & 127) << num;
                num += 7;
                if ((b & 128) == 0)
                    return true;
            }
            value = 0UL;
            return false;
        }

        public static void WriteBytes(this GrowingSpanBuffer buffer, GrowingSpanBuffer other)
            => buffer.WriteBytes(other.Data);

        public static void WriteVarBytes(this GrowingSpanBuffer buffer, ReadOnlySpan<byte> bytes)
        {
            buffer.WriteVarInt(bytes.Length);
            buffer.WriteBytes(bytes);
        }

        public static ReadOnlySpan<byte> ReadVarBytes(this SpanBufferReader bufferReader)
        {
            var length = bufferReader.ReadVarUInt();
            return bufferReader.ReadBytes((int)length);
        }

        public static void WriteString(this GrowingSpanBuffer buffer, string value)
        {
            buffer.WriteInt32(value.Length);
            buffer.WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public static string ReadString(this SpanBufferReader bufferReader, int maxLength = 65535)
        {
            var length = bufferReader.ReadInt32();
            if (length <= 0 | length > maxLength)
                return string.Empty;
            return Encoding.UTF8.GetString(bufferReader.ReadBytes(length));
        }

        public static void WriteIPEndPoint(this GrowingSpanBuffer buffer, IPEndPoint ipEndPoint)
        {
            buffer.WriteString(ipEndPoint.Address.ToString());
            buffer.WriteInt32(ipEndPoint.Port);
        }

        public static IPEndPoint ReadIPEndPoint(this SpanBufferReader bufferReader)
        {
            if (!IPAddress.TryParse(bufferReader.ReadString(512), out var address))
                throw new ArgumentException("Failed to parse IP address");
            var port = bufferReader.ReadInt32();
            return new IPEndPoint(address, port);
        }
    }
}
