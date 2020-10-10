using System.IO;
using SmBios.Data;

namespace SmBios.Reader.Readers
{
    internal class ReaderMemory<T>: TableReader<T> where T: TableMemoryDevice
    {
        internal ReaderMemory(BinaryReader reader, TableHeader header): base(reader, header)
        {
            // 2.1+
            if (header.VersionBi >= Const.SMBIOS_2_1)
            {
                Result.PhysicalArrayHandle = reader.ReadUInt16();
                Result.ErrorInformationHandle = reader.ReadUInt16();
                Result.TotalWidth = reader.ReadUInt16();
                Result.DataWidth = reader.ReadUInt16();
                Result.Size = reader.ReadUInt16();
                Result.FormFactor = reader.ReadByte();
                Result.DeviceSet = reader.ReadByte();
                int deviceLocator = reader.ReadByte();
                int bankLocator = reader.ReadByte();
                Result.MemoryType = reader.ReadByte();
                Result.TypeDetail = reader.ReadUInt16();

                Result.DeviceLocator = header.GetString(deviceLocator);
                Result.BankLocator = header.GetString(bankLocator);
            }
            // 2.3+
            if (header.VersionBi >= Const.SMBIOS_2_3)
            {
                Result.Speed = reader.ReadUInt16();
                int manufacturer = reader.ReadByte();
                int serialNumber = reader.ReadByte();
                int assetTagNumber = reader.ReadByte();
                int partNumber = reader.ReadByte();

                Result.Manufacturer = header.GetString(manufacturer);
                Result.SerialNumber = header.GetString(serialNumber);
                Result.AssetTagNumber = header.GetString(assetTagNumber);
                Result.PartNumber = header.GetString(partNumber);
            }
            // 2.6+
            if (header.VersionBi >= Const.SMBIOS_2_6)
            {
                Result.Attributes = reader.ReadByte();
            }
            // 2.7+
            if (header.VersionBi >= Const.SMBIOS_2_7)
            {
                Result.ExtendedSize = reader.ReadUInt32();
                Result.ConfiguredClockSpeed = reader.ReadUInt16();
            }
            // 2.8+
            if (header.VersionBi >= Const.SMBIOS_2_8)
            {
                Result.MinimumVoltage = reader.ReadUInt16();
                Result.MinimumVoltage = reader.ReadUInt16();
                Result.ConfiguredVoltage = reader.ReadUInt16();
            }
        }
    }
}
