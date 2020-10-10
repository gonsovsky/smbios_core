using System.IO;
using SmBios.Data;

namespace SmBios.Reader.Readers
{
    internal class ReaderBaseboard<T>: TableReader<T> where T: TableBaseboard
    {
        internal ReaderBaseboard(BinaryReader reader, TableHeader header): base(reader, header)
        {
            // 2.0+
            if (header.VersionBi >= Const.SMBIOS_2_0)
            {
                int manufacturer = reader.ReadByte();
                int product = reader.ReadByte();
                int version = reader.ReadByte();
                int serialNumber = reader.ReadByte();
                int assetTag = reader.ReadByte();
                Result.FeatureFlags = reader.ReadByte();
                int locationInChassis = reader.ReadByte();
                Result.ChassisHandle = reader.ReadUInt16();
                Result.BoardType = reader.ReadByte();
                //NoOfContainedObjectHandles = reader.ReadByte();
                //ContainedObjectHandles = (uint16_t*)ptr_;
                //ptr_ += entry_.data.baseboard.NoOfContainedObjectHandles * sizeof(uint16_t);
                Result.Manufacturer = header.GetString(manufacturer);
                Result.Product = header.GetString(product);
                Result.Version = header.GetString(version);
                Result.SerialNumber = header.GetString(serialNumber);
                Result.AssetTag = header.GetString(assetTag);
                Result.LocationInChassis = header.GetString(locationInChassis);
            }
        }
    }
}
