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

    }
}
