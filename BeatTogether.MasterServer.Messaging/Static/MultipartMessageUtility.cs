using System;
using System.Collections.Generic;
using BeatTogether.MasterServer.Messaging.Implementations.Messages;

namespace BeatTogether.MasterServer.Messaging.Static
{
    public static class MultipartMessageUtility
    {
        private const int _bufferSliceLength = 384;

        public static IEnumerable<MultipartMessage> GetMultipartMessages(uint multipartMessageId, byte[] buffer)
        {
            for (var bufferSliceStart = 0;
                 bufferSliceStart < buffer.Length;
                 bufferSliceStart += _bufferSliceLength)
            {
                var bufferSliceLength = _bufferSliceLength;
                if ((bufferSliceStart + bufferSliceLength) > buffer.Length)
                    bufferSliceLength = buffer.Length - bufferSliceLength;
                var bufferSlice = new byte[bufferSliceLength];
                Array.Copy(buffer, bufferSliceStart, bufferSlice, 0, bufferSliceLength);
                yield return new MultipartMessage()
                {
                    MultipartMessageId = multipartMessageId,
                    Offset = (uint)bufferSliceStart,
                    Length = (uint)bufferSliceLength,
                    TotalLength = (uint)buffer.Length,
                    Data = bufferSlice
                };
            }
        }
    }
}
