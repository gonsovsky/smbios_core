using System.IO;
using SmBios.Data;

namespace SmBios.Reader.Readers
{
    internal class ReaderProcessor<T>: TableReader<T> where T: TableProcessor
    {
        internal ReaderProcessor(BinaryReader reader, TableHeader header): base(reader, header)
        {
            // VersionBI 2.0
            if (header.VersionBi >= Const.SMBIOS_2_0)
            {
                int socketDesignation = reader.ReadByte();
                Result.ProcessorType = reader.ReadByte();
                Result.ProcessorFamily = reader.ReadByte();
                int processorManufacturer = reader.ReadByte();
                for (int i = 0; i < 8; ++i)
                    Result.ProcessorId[i] = reader.ReadByte();
                int processorVersion = reader.ReadByte();
                Result.Voltage = reader.ReadByte();
                Result.ExternalClock = reader.ReadUInt16();
                Result.MaxSpeed = reader.ReadUInt16();
                Result.CurrentSpeed = reader.ReadUInt16();
                Result.Status = reader.ReadByte();
                Result.ProcessorUpgrade = reader.ReadByte();

                Result.SocketDesignation = header.GetString(socketDesignation);
                Result.ProcessorManufacturer = header.GetString(processorManufacturer);
                Result.ProcessorVersion = header.GetString(processorVersion);
            }
            if (header.VersionBi >= Const.SMBIOS_2_1)
            {
                Result.L1CacheHandle = reader.ReadUInt16();
                Result.L2CacheHandle = reader.ReadUInt16();
                Result.L3CacheHandle = reader.ReadUInt16();
            }
            if (header.VersionBi >= Const.SMBIOS_2_3)
            {
                int serialNumber = reader.ReadByte();
                int assetTagNumber = reader.ReadByte();
                int partNumber = reader.ReadByte();

                Result.SerialNumber = header.GetString(serialNumber);
                Result.AssetTagNumber = header.GetString(assetTagNumber);
                Result.PartNumber = header.GetString(partNumber);
            }
            if (header.VersionBi >= Const.SMBIOS_2_5)
            {
                Result.CoreCount = reader.ReadByte();
                Result.CoreEnabled = reader.ReadByte();
                Result.ThreadCount = reader.ReadByte();
                Result.ProcessorCharacteristics = reader.ReadUInt16();
            }
            if (header.VersionBi >= Const.SMBIOS_2_6)
            {
                Result.ProcessorFamily2 = reader.ReadUInt16();
            }
            if (header.VersionBi >= Const.SMBIOS_3_0)
            {
                Result.CoreCount2 = reader.ReadUInt16();
                Result.CoreEnabled2 = reader.ReadUInt16();
                Result.ThreadCount2 = reader.ReadUInt16();
            }
        }
    }
}
