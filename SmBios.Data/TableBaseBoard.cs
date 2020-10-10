using System.Collections.Generic;

namespace SmBios.Data
{
    public class TableBaseboard : Table
    {
        public string Manufacturer;
        public string Product;
        public string Version;
        public string SerialNumber;
        public string AssetTag;
        public byte FeatureFlags;
        public string LocationInChassis;
        public ushort ChassisHandle;

        #region BoardType
        public byte BoardType;

        public static Dictionary<byte, string> BoardTypeDictionary = new Dictionary<byte, string>()
            {
                {0x01, "Unknown"},
                {0x02, "Other"},
                {0x03, "Server Blade"},
                {0x04, "Connectivity Switch"},
                {0x05, "System Management Module"},
                {0x06, "Processor Module"},
                {0x07, "I/O Module"},
                {0x08, "Memory Module"},
                {0x09, "Daughter board"},
                {0x0A, "Motherboard (includes processor, memory, and I/O)"},
                {0x0B, "Processor/Memory Module"},
                {0x0C, "Processor/IO Module"},
                {0x0D, "Interconnect board"},
            };

        public string BoardTypeDisp =>
            BoardTypeDictionary.TryGetValue(BoardType, out var value)
                ? value
                : BoardType.ToString() + " :Table 15";
        #endregion

        //public byte NoOfContainedObjectHandles;
        //public ushort ContainedObjectHandles;
    }
}
