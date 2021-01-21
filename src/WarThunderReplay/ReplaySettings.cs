using System;

namespace WarThunderReplay
{
    public class ReplaySettings
    {

        public string battleType { get; }

        public string missionNameLocalization { get; }

        public string weather { get; }

        public double time { get; }

        public string missionName { get; }

        public string missionFile { get; }

        public string map { get; }

        public ReplaySettings(ByteStream binaryData)
        {
            this.map = BitConverter.ToString(binaryData.GetBytes(128));
            this.missionFile = BitConverter.ToString(binaryData.GetBytes(260));
            this.missionName = BitConverter.ToString(binaryData.GetBytes(128));
            this.time = BitConverter.ToDouble(binaryData.GetBytes(128));
            this.weather = BitConverter.ToString(binaryData.GetBytes(32));
            var unknown0 = binaryData.GetBytes(92);
            this.missionNameLocalization = BitConverter.ToString(binaryData.GetBytes(128));
            var unknown1 = binaryData.GetBytes(60);
            this.battleType = BitConverter.ToString(binaryData.GetBytes(128));
        }


    }
}
