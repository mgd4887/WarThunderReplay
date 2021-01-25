using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using Ionic.Zlib;
using CompressionMode = Ionic.Zlib.CompressionMode;

namespace WarThunderReplay
{
    class Replay
    {
        private readonly ByteStream _binaryData;
        private readonly List<Packet> _binaryPackets;
        private readonly List <TypedPacket> _typedPackets;
        private string _fileName;
        private readonly byte[] _replayMagicBytes = { 0xE5, 0xAC, 0x00, 0x10 };
        private readonly byte[] _blkMagicBytes = { 0x00, 0x42, 0x42, 0x46 };

        public  ReplaySettings ReplaySettings { get; private set; }

        public int ReplayFileVersion { get; private set; }

        public byte[] ResultsblkFile { get; private set; }

        public byte[] MissionblkFile { get; private set; }

        public Replay(string fileName)
        {
            _fileName = fileName;
            _binaryData = GetBinaryData(fileName);
            _binaryPackets = new List<Packet>();
            _typedPackets = new List<TypedPacket>();
        }

        /// <summary>
        /// parses the replay wprl file.
        /// </summary>
        public void Parse()
        {
            var replayMagic = _binaryData.GetBytes(4);
            if (replayMagic.Equals(_replayMagicBytes))
            {
                throw new InvalidDataException("Not a replay file; magic number mismatch");
            }

            ReplayFileVersion = BitConverter.ToInt32(_binaryData.GetBytes(4));

            ReplaySettings = new ReplaySettings(_binaryData);

            var _ = _binaryData.EndAndGetCurrentSegmentBytes();

            var blkMagic = _binaryData.GetBytes(4);
            if (blkMagic.Equals(_blkMagicBytes))
            {
                throw new InvalidDataException("BLK file not found at expected offset");
            }

            var unknown2 = _binaryData.GetBytes(4);
            var missionblkLength = BitConverter.ToUInt32(_binaryData.GetBytes(4));
            var missionblkData = _binaryData.GetBytes((int) missionblkLength);

            MissionblkFile = _binaryData.EndAndGetCurrentSegmentBytes(); // all BLK data in one var;

            var replayData = DecompressZLibStream(_binaryData);
            var packets = replayData.Item1;
            ResultsblkFile = replayData.Item2;

            // parse packets last
            ParsePackets(new ByteStream(packets));

        }



        private (byte[], byte[]) DecompressZLibStream(ByteStream binaryData)
        {
            var input = binaryData.GetAllBytesLeft();
            byte[] packets;
            byte[] results;

            using (ZlibStream zlibStream = new ZlibStream(new MemoryStream(input), CompressionMode.Decompress))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    zlibStream.CopyTo(outStream);
                    var totalIn = zlibStream.TotalIn;
                    ByteStream newStream = new ByteStream(input);
                    newStream.Seek((int)totalIn); /* lossy, but almost impossible we would ever see a 2.14GB+ file.
                                                    this is also the location of the end of the packet deflate stream. */

                    results = newStream.GetAllBytesLeft();
                    packets = outStream.ToArray();

                }
            }

            return (packets, results);
        }

        /// <summary>
        /// separate out each packet.
        /// </summary>
        /// <param name="packets"></param>
        public void ParsePackets(ByteStream packets)
        {
            int offset = 0x0;
            int i = 0;
            while (packets.HasMore())
            {
                ExtractNextPacket(packets);
                i++;
            }

            i = 0;
            foreach (var binaryPacket in _binaryPackets)
            {
                _typedPackets.Add(binaryPacket.Classify(i));
                i++;
            }

        }

        private void ExtractNextPacket(ByteStream byteStream)
        {
            var magic = byteStream.GetByte();
            var offset = byteStream.Index;
            var magicNumber = int.Parse(magic.ToString());
            ulong packetLength = 0;
            int lengthOffset = 0;
            if ((magicNumber & 0x80) > 1) // 1000 0000
            {
                var test = (magicNumber - 0x80);
                packetLength = (ulong) (magicNumber - 0x80);
                lengthOffset = 1;
            }
            else if ((magicNumber & 0x40) > 1) // 0100 0000
            {
                var nextBytes = byteStream.GetBytes(1);
                packetLength = GetPacketLength(magicNumber, 0x40, nextBytes);
                lengthOffset = 2;
            }
            else if ((magicNumber & 0x20) > 1) // 0010 0000
            {
                var nextBytes = byteStream.GetBytes(2);
                packetLength = GetPacketLength(magicNumber, 0x20, nextBytes);
                lengthOffset = 3;
            }
            else if ((magicNumber & 0x10) > 1) // 0001 0000
            {
                var nextBytes = byteStream.GetBytes(3);
                packetLength = GetPacketLength(magicNumber, 0x10, nextBytes);
                lengthOffset = 4;
            }

            if (packetLength > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            byteStream.GetBytes((int) packetLength);
            _binaryPackets.Add(new Packet(byteStream.EndAndGetCurrentSegmentBytes(), lengthOffset, offset));
        }

        /// <summary>
        /// return the given hexbytes as a ulong.
        /// </summary>
        /// <param name="magicNumber">the first byte of the length</param>
        /// <param name="andNumber">the number that &ed with the magic number</param>
        /// <param name="nextBytes">the next bytes based on the the andNumber</param>
        /// <returns>the length of the packet</returns>
        private ulong GetPacketLength(in int magicNumber, int andNumber, byte[] nextBytes)
        {
            var hexString = BitConverter.ToString(nextBytes).Replace("-", string.Empty);
            hexString = (magicNumber - andNumber).ToString("X") + hexString;
            ulong output = ulong.Parse(hexString, NumberStyles.AllowHexSpecifier);
            return output;
        }

        /// <summary>
        /// just reads the file to the byte array
        /// </summary>
        /// <param name="fileName">the name of the file</param>
        /// <returns>the byte data of the file</returns>
        private ByteStream GetBinaryData(string fileName)
        {
            if (File.Exists(fileName))
            {
                // We should never have a replay that is greater than 4.2 GB (this method's limit). So this should be fine.
                // replays are in the 1-20 MB range at most
                Byte[] allBytes = File.ReadAllBytes(fileName);
                return new ByteStream(allBytes);
            }

            // Guarantee that this is the exception thrown 
            throw new FileNotFoundException();
        }
    }
}
