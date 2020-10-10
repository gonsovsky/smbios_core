using System.IO;
using SmBios.Data;

namespace SmBios.Reader.Readers
{
    internal class ReaderBios<T>: TableReader<T> where T: TableBios
    {
        internal ReaderBios(BinaryReader reader, TableHeader header): base(reader, header)
        {
            // 2.0+
            if (header.VersionBi >= Const.SMBIOS_2_0)
            {
                int vendor = reader.ReadByte();
                int biosVersion = reader.ReadByte();
                Result.BiosStartingSegment = reader.ReadUInt16();
                int biosReleaseDate = reader.ReadByte();
                Result.BiosromSize = reader.ReadByte();
                for (var i = 0; i < 8; ++i)
                    Result.BiosCharacteristics[i] = reader.ReadByte();

                Result.Vendor = header.GetString(vendor);
                Result.BiosVersion = header.GetString(biosVersion);
                Result.BiosReleaseDate = header.GetString(biosReleaseDate);
            }
            // 2.4+
            if (header.VersionBi >= Const.SMBIOS_2_4)
            {
                Result.ExtensionByte1 = reader.ReadByte();
                Result.ExtensionByte2 = reader.ReadByte();
                Result.SystemBiosMajorRelease = reader.ReadByte();
                Result.SystemBiosMinorRelease = reader.ReadByte();
                Result.EmbeddedFirmwareMajorRelease = reader.ReadByte();
                Result.EmbeddedFirmwareMinorRelease = reader.ReadByte();
            }
        }
    }
}
