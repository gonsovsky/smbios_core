using System.IO;
using SmBios.Data;

namespace SmBios.Reader.Readers
{
    internal class ReaderPhysicalMemory<T>: TableReader<T> where T: TablePhysicalMemory
    {
        internal ReaderPhysicalMemory(BinaryReader reader, TableHeader header): base(reader, header)
        {
            if (header.VersionBi >= Const.SMBIOS_2_1)
            {
                Result.Location = reader.ReadByte();
                Result.Use = reader.ReadByte();
                Result.ErrorCorrection = reader.ReadByte();
                Result.MaximumCapacity = reader.ReadUInt32();
                Result.ErrorInformationHandle = reader.ReadUInt16();
                Result.NumberDevices = reader.ReadUInt16();
            }
            // 2.7+
            if (header.VersionBi >= Const.SMBIOS_2_7)
            {
                Result.ExtendedMaximumCapacity = reader.ReadUInt64();
            }
        }
    }
}
