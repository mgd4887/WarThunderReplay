using System;
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
            return GetBytes(_bytes.Length - Index);
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
            var output = new StringBuilder();
            foreach (var b in _bytes)
            {
                output.Append(b.ToString("X2"));
            }

            return output.ToString();
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
