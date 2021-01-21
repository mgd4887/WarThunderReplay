using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace WarThunderReplay
{
    class Replay
    {
        private ByteStream _binaryData;
        private List<Packet> _binaryPackets;
        private List <TypedPacket> _typedPackets;
        private string _fileName;
        private readonly byte[] replayMagicBytes = { 0xE5, 0xAC, 0x00, 0x10 };
        private readonly byte[] blkMagicBytes = { 0x00, 0x42, 0x42, 0x46 };

        public  ReplaySettings replaySettings { get; private set; }

        public byte[] resultsblkFile { get; private set; }

        public byte[] missionblkFile { get; private set; }

        public Replay(string fileName)
        {
            _fileName = fileName;
            _binaryData = GetBinaryData(fileName);
            _binaryPackets = new List<Packet>();
            _typedPackets = new List<TypedPacket>();
        }

        /// <summary>
        /// parses the header of a replay wprl file.
        /// </summary>
        public void ParseHeader()
        {
            var replayMagic = _binaryData.GetBytes(4);
            if (replayMagic.Equals(replayMagicBytes))
            {
                throw new InvalidDataException("Not a replay file; magic number mismatch");
            }

            var replayFileVersion = _binaryData.GetBytes(4);

            this.replaySettings = new ReplaySettings(_binaryData);

            var _ = _binaryData.EndAndGetCurrentSegmentBytes();

            var blkMagic = _binaryData.GetBytes(4);
            if (blkMagic.Equals(blkMagicBytes))
            {
                throw new InvalidDataException("BLK file not found at expected offset");
            }

            var unknown2 = _binaryData.GetBytes(4);
            var missionblkLength = BitConverter.ToUInt32(_binaryData.GetBytes(4));
            var missionblkData = _binaryData.GetBytes((int) missionblkLength); // lossy conversion. 

            this.missionblkFile = _binaryData.EndAndGetCurrentSegmentBytes(); // all BLK data in one var;

            var Packets = DecompressZLibStream(_binaryData); // TODO: how long is this part?

            var bbf = new byte[] { 0x00, 0x42, 0x42, 0x46, 0x03, 0x00 };
            var lastBbfIndex = _binaryData.BackSearch(bbf);

            if (lastBbfIndex > 0x445) // server replays will not contain results. 
            {
                _binaryData.Seek(lastBbfIndex);
                var hasResults = true;
                var resultsblkMagic = _binaryData.GetBytes(4);
                var resultsUnknown = _binaryData.GetBytes(4);
                var resultsblkLength = BitConverter.ToUInt32(_binaryData.GetBytes(4));
                var resultsblkData = _binaryData.GetBytes((int) resultsblkLength); // lossy conversion.

                this.resultsblkFile = _binaryData.EndAndGetCurrentSegmentBytes(); // all BLK data in one var;
            }




        }



        private byte[] DecompressZLibStream(ByteStream binaryData)
        {
            var input = binaryData.GetAllBytesLeft();
            input = input.Skip(2).ToArray(); // remove zlib header.
            
            using (var inputStream = new MemoryStream(input))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (DeflateStream decompressionStream =
                        new DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(outputStream);
                    }

                    var output = outputStream.ToArray();
                    return output;
                }
            }
            return null;
        }

        /// <summary>
        /// separate out each packet.
        /// </summary>
        public void Parse()
        {
            int offset = 0x0;
            int i = 0;
            while (_binaryData.HasMore())
            {
                if (i == 129)
                {
                    Console.WriteLine("");
                }
                ExtractNextPacket();
                i++;
            }

            i = 0;
            foreach (var binaryPacket in _binaryPackets)
            {
                _typedPackets.Add(binaryPacket.Classify(i));
                i++;
            }

        }

        private void ExtractNextPacket()
        {
            var magic = _binaryData.GetByte();
            var offset = _binaryData.Index;
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
                var nextBytes = _binaryData.GetBytes(1);
                packetLength = GetPacketLength(magicNumber, 0x40, nextBytes);
                lengthOffset = 2;
            }
            else if ((magicNumber & 0x20) > 1) // 0010 0000
            {
                var nextBytes = _binaryData.GetBytes(2);
                packetLength = GetPacketLength(magicNumber, 0x20, nextBytes);
                lengthOffset = 3;
            }
            else if ((magicNumber & 0x10) > 1) // 0001 0000
            {
                var nextBytes = _binaryData.GetBytes(3);
                packetLength = GetPacketLength(magicNumber, 0x10, nextBytes);
                lengthOffset = 4;
            }

            if (packetLength > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }

            _binaryData.GetBytes((int) packetLength);
            _binaryPackets.Add(new Packet(_binaryData.EndAndGetCurrentSegmentBytes(), lengthOffset, offset));
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
