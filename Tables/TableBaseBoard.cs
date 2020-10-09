using System.Collections.Generic;
using System.IO;

namespace SmBiosCore.Tables
{
    public class TableBaseboard : SmBiosTable
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

        internal TableBaseboard(BinaryReader reader, SmBiosTableHeader header)
        {
            // 2.0+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_0)
            {
                int Manufacturer_ = reader.ReadByte();
                int Product_ = reader.ReadByte();
                int Version_ = reader.ReadByte();
                int SerialNumber_ = reader.ReadByte();
                int AssetTag_ = reader.ReadByte();
                FeatureFlags = reader.ReadByte();
                int LocationInChassis_ = reader.ReadByte();
                ChassisHandle = reader.ReadUInt16();
                BoardType = reader.ReadByte();
                //NoOfContainedObjectHandles = reader.ReadByte();
                //ContainedObjectHandles = (uint16_t*)ptr_;
                //ptr_ += entry_.data.baseboard.NoOfContainedObjectHandles * sizeof(uint16_t);
                Manufacturer = header.GetString(Manufacturer_);
                Product = header.GetString(Product_);
                Version = header.GetString(Version_);
                SerialNumber = header.GetString(SerialNumber_);
                AssetTag = header.GetString(AssetTag_);
                LocationInChassis = header.GetString(LocationInChassis_);
            }
        }
    }
}
