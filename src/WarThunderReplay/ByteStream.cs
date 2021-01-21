﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarThunderReplay
{
    public class ByteStream
    {
        public int Index { get; private set; }

        private readonly byte[] _bytes;
        private readonly List<byte> _currentSegmentBytes;

        public ByteStream(byte[] bytes)
        {
            this._bytes = bytes;
            this.Index = 0;
            this._currentSegmentBytes = new List<byte>();
        }

        public byte GetByte()
        {
            return GetBytes(1)[0];
        }

        public byte[] GetAllBytesLeft()
        {
            return GetBytes(_bytes.Length - 1 - Index);
        }

        public byte[] GetBytes(int count)
        {
            byte[] output = _bytes.Skip(Index).Take(count).ToArray();
            Index += count;
            _currentSegmentBytes.AddRange(output);
            return output;
        }

        public bool HasMore()
        {
            return !(Index >= _bytes.Length);
        }

        public byte[] EndAndGetCurrentSegmentBytes()
        {
            var output = _currentSegmentBytes.ToArray();
            _currentSegmentBytes.Clear();
            return output;
        }

        public string ToHex()
        {
            string output = "";
            foreach (var b in _bytes)
            {
                output += b.ToString("X2");
            }

            return output;
        }

        /// <summary>
        /// Returns the first instance of a string of bytes in the bytestream and returns its offset.
        /// </summary>
        /// <param name="bytesToFind">The bytes being searched for.</param>
        /// <returns>returns -1 is the string is not found
        /// otherwise it returns the offset of the </returns>
        public int BackSearch(byte[] bytesToFind)
        {
            var length = _bytes.Length;
            var captureLength = bytesToFind.Length;
            for (int i = length - captureLength; i >= 0; i--)
            {
                var selected = _bytes.Skip(i).Take(captureLength);
                if (selected.SequenceEqual(bytesToFind))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Seeks to the given index
        /// WILL CLEAR SAVED BYTES.
        /// </summary>
        /// <param name="index">The index to seek to</param>
        public void Seek(in int index)
        {
            _ = EndAndGetCurrentSegmentBytes();
            this.Index = index;
        }
    }
}
