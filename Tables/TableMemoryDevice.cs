using System.IO;

namespace SmBiosCore.Tables
{
    public class TableMemoryDevice : SmBiosTable
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

        internal TableMemoryDevice(BinaryReader reader, SmBiosTableHeader header)
        {
            // 2.1+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_1)
            {
                PhysicalArrayHandle = reader.ReadUInt16();
                ErrorInformationHandle = reader.ReadUInt16();
                TotalWidth = reader.ReadUInt16();
                DataWidth = reader.ReadUInt16();
                Size = reader.ReadUInt16();
                FormFactor = reader.ReadByte();
                DeviceSet = reader.ReadByte();
                int DeviceLocator_ = reader.ReadByte();
                int BankLocator_ = reader.ReadByte();
                MemoryType = reader.ReadByte();
                TypeDetail = reader.ReadUInt16();

                DeviceLocator = header.GetString(DeviceLocator_);
                BankLocator = header.GetString(BankLocator_);
            }
            // 2.3+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_3)
            {
                Speed = reader.ReadUInt16();
                int Manufacturer_ = reader.ReadByte();
                int SerialNumber_ = reader.ReadByte();
                int AssetTagNumber_ = reader.ReadByte();
                int PartNumber_ = reader.ReadByte();

                Manufacturer = header.GetString(Manufacturer_);
                SerialNumber = header.GetString(SerialNumber_);
                AssetTagNumber = header.GetString(AssetTagNumber_);
                PartNumber = header.GetString(PartNumber_);
            }
            // 2.6+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_6)
            {
                Attributes = reader.ReadByte();
            }
            // 2.7+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_7)
            {
                ExtendedSize = reader.ReadUInt32();
                ConfiguredClockSpeed = reader.ReadUInt16();
            }
            // 2.8+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_8)
            {
                MinimumVoltage = reader.ReadUInt16();
                MinimumVoltage = reader.ReadUInt16();
                ConfiguredVoltage = reader.ReadUInt16();
            }
        }
    }
}
