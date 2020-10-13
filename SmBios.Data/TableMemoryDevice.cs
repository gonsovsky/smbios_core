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

        [Browsable(false)]
        public ushort DataWidth { get; set; }

        [Browsable(true)]
        public ushort Size { get; set; }

        [Browsable(false)]
        public byte FormFactor { get; set; }

        [Browsable(false)]
        public byte DeviceSet { get; set; }

        [DisplayName("Размещение")]
        [Browsable(true)]
        public string DeviceLocator { get; set; }

        [DisplayName("Банк")]
        [Browsable(true)]
        public string BankLocator { get; set; }

        [Browsable(false)]
        public byte MemoryType { get; set; }

        [Browsable(false)]
        public ushort TypeDetail { get; set; }
        // 2.3+

        [Browsable(false)]
        public ushort Speed { get; set; }

        [DisplayName("Производитель")]
        [Browsable(true)]
        public string Manufacturer { get; set; }


        [DisplayName("Серийный номер")]
        [Browsable(true)]
        public string SerialNumber { get; set; }

        [DisplayName("Тег имущества")]
        [Browsable(false)]
        public string AssetTagNumber { get; set; }

        [Browsable(true)]
        public string PartNumber { get; set; }
        // 2.6+

        [Browsable(false)]
        public byte Attributes { get; set; }
        // 2.7+

        [Browsable(true)]
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
