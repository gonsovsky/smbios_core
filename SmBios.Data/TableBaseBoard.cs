using System.Collections.Generic;
using System.ComponentModel;

namespace SmBios.Data
{
    public class TableBaseboard : Table
    {
        [DisplayName("Производитель")]
        public string Manufacturer { get; set; }

        [DisplayName("Продукт")]
        public string Product { get; set; }

        [DisplayName("Версия")]
        public string Version { get; set; }

        [DisplayName("Серийный номер")]
        public string SerialNumber { get; set; }

        [Browsable(false)]
        public string AssetTag { get; set; }

        [Browsable(false)]
        public byte FeatureFlags { get; set; }

        [Browsable(false)]
        public string LocationInChassis { get; set; }

        [Browsable(false)]
        public ushort ChassisHandle { get; set; }

        #region BoardType
        [Browsable(false)]
        public byte BoardType { get; set; }

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

        [DisplayName("Тип")]
        public string BoardTypeDisp =>
            BoardTypeDictionary.TryGetValue(BoardType, out var value)
                ? value
                : BoardType.ToString() + " :Table 15";
        #endregion

        //public byte NoOfContainedObjectHandles;
        //public ushort ContainedObjectHandles;

        [Browsable(false)]
        public override string Name => Version;
    }
}
