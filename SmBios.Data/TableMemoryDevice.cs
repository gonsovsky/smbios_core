using System.Collections.Generic;
using System.ComponentModel;

namespace SmBios.Data
{
    public class TableMemoryDevice : Table
    {
        // 2.1+
        [Browsable(false)]
        public ushort PhysicalArrayHandle { get; set; }

        [Browsable(false)]
        public ushort ErrorInformationHandle { get; set; }

        [Browsable(false)]
        public ushort TotalWidth { get; set; }

        [DisplayName("Общая ширина")]
        [Browsable(true)]
        public string TotalWidthDisp => $"{TotalWidth} бит";

        [Browsable(false)]
        public ushort DataWidth { get; set; }

        [DisplayName("Ширина данных")]
        [Browsable(true)]
        public string DataWidthDisp => $"{DataWidth} бит";

        [Browsable(false)]
        public ushort Size { get; set; }

        [DisplayName("Размер")]
        [Browsable(true)] public string SizeDisp => $"{Size} МБ";

        #region FormFactor
        [Browsable(false)]
        public byte FormFactor { get; set; }

        public static Dictionary<byte, string> FormFactorDictionary = new Dictionary<byte, string>()
        {
            {0x01, "Other"},
            {0x02, "Unknown"},
            {0x03, "SIMM"},
            {0x04, "SIP"},
            {0x05, "Chip"},
            {0x06, "DIP"},
            {0x07, "ZIP"},
            {0x08, "Proprietary Card"},
            {0x09, "DIMM"},
            {0x0A, "TSOP"},
            {0x0B, "Row of chips"},
            {0x0C, "RIMM"},
            {0x0D, "SODIMM"},
            {0x0E, "SRIMM"},
            {0x0F, "FB-DIMM"}
        };

        [DisplayName("Тип памяти")]
        public string FormFactorDisp =>
            FormFactorDictionary.TryGetValue(FormFactor, out var value)
                ? value
                : FormFactor.ToString() + " :Table 75";
        #endregion

        [Browsable(false)]
        public byte DeviceSet { get; set; }

        [DisplayName("Размещение")]
        [Browsable(true)]
        public string DeviceLocator { get; set; }

        [DisplayName("Банк")]
        [Browsable(true)]
        public string BankLocator { get; set; }

        #region MemoryType
        [Browsable(false)]
        public byte MemoryType { get; set; }

        public static Dictionary<byte, string> MemoryTypeDictionary = new Dictionary<byte, string>()
        {
            {0x01, "Other"},
            {0x02, "Unknown"},
            {0x03, "DRAM"},
            {0x04, "EDRAM"},
            {0x05, "VRAM"},
            {0x06, "SRAM"},
            {0x07, "RAM"},
            {0x08, "ROM"},
            {0x09, "FLASH"},
            {0x0A, "EEPROM"},
            {0x0B, "EEPROM"},
            {0x0C, "EPROM"},
            {0x0D, "CDRAM"},
            {0x0E, "3DRAM"},
            {0x0F, "SDRAM"},
            {0x10, "SGRAM"},
            {0x11, "RDRAM"},
            {0x12, "DDR"},
            {0x13, "DDR2"},
            {0x14, "DDR2 FB-DIMM"},
            {0x15, "Reserved"},
            {0x16, "Reserved"},
            {0x17, "Reserved"},
            {0x18, "DDR3"},
            {0x19, "FBD2"},
            {0x1A, "DDR4"},
            {0x1B, "LPDDR"},
            {0x1C, "LPDDR2"},
            {0x1D, "LPDDR3"},
            {0x1E, "LPDDR4"},
            {0x1F, "Logical non-volatile device"},
        };

        [DisplayName("Тип памяти")]
        public string MemoryTypeDisp =>
            MemoryTypeDictionary.TryGetValue(MemoryType, out var value)
                ? value
                : MemoryType.ToString() + " :Table 76";
        #endregion

        [Browsable(false)]
        public ushort TypeDetail { get; set; }
        // 2.3+

        [Browsable(false)]
        public ushort Speed { get; set; }

        [DisplayName("Максимальная частота")]
        [Browsable(true)]
        public string SpeedDisp => $"{Speed} MT/s";

        [DisplayName("Производитель")]
        [Browsable(true)]
        public string Manufacturer { get; set; }

        [DisplayName("Серийный номер")]
        [Browsable(true)]
        public string SerialNumber { get; set; }

        [DisplayName("Тег имущества")]
        [Browsable(true)]
        public string AssetTagNumber { get; set; }

        [DisplayName("Шифр компонента")]
        [Browsable(true)]
        public string PartNumber { get; set; }
        // 2.6+

        [Browsable(false)]
        public byte Attributes { get; set; }
        // 2.7+

        [Browsable(false)]
        public uint ExtendedSize { get; set; }

        [Browsable(false)]
        public ushort ConfiguredClockSpeed { get; set; }
        // 2.8+

        [Browsable(false)]
        public ushort MinimumVoltage { get; set; }

        [Browsable(false)]
        public ushort MaximumVoltage { get; set; }

        [Browsable(false)]
        public ushort ConfiguredVoltage { get; set; }

        [Browsable(false)]
        public override string Name => DeviceLocator;
    }
}
