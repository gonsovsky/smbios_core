namespace SmBios.Data
{
    public class TableMemoryDevice : Table
    {
        // 2.1+
        public ushort PhysicalArrayHandle;
        public ushort ErrorInformationHandle;
        public ushort TotalWidth;
        public ushort DataWidth;
        public ushort Size;
        public byte FormFactor;
        public byte DeviceSet;
        public string DeviceLocator;
        public string BankLocator;
        public byte MemoryType;
        public ushort TypeDetail;
        // 2.3+
        public ushort Speed;
        public string Manufacturer;
        public string SerialNumber;
        public string AssetTagNumber;
        public string PartNumber;
        // 2.6+
        public byte Attributes;
        // 2.7+
        public uint ExtendedSize;
        public ushort ConfiguredClockSpeed;
        // 2.8+
        public ushort MinimumVoltage;
        public ushort MaximumVoltage;
        public ushort ConfiguredVoltage;
    }
}
