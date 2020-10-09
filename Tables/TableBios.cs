using System.IO;

namespace SmBiosCore.Tables
{
    public class TableBios : SmBiosTable
    {
        public string Vendor;
        public string BiosVersion;
        public ushort BiosStartingSegment;
        public string BiosReleaseDate;
        public byte BiosromSize;
        public byte[] BiosCharacteristics = new byte[8];
        public byte ExtensionByte1;
        public byte ExtensionByte2;
        public byte SystemBiosMajorRelease;
        public byte SystemBiosMinorRelease;
        public byte EmbeddedFirmwareMajorRelease;
        public byte EmbeddedFirmwareMinorRelease;

        internal TableBios(BinaryReader reader, SmBiosTableHeader header)
        {
            // 2.0+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_0)
            {
                int Vendor_ = reader.ReadByte();
                int BiosVersion_ = reader.ReadByte();
                BiosStartingSegment = reader.ReadUInt16();
                int BiosReleaseDate_ = reader.ReadByte();
                BiosromSize = reader.ReadByte();
                for (var i = 0; i < 8; ++i)
                    BiosCharacteristics[i] = reader.ReadByte();

                Vendor = header.GetString(Vendor_);
                BiosVersion = header.GetString(BiosVersion_);
                BiosReleaseDate = header.GetString(BiosReleaseDate_);
            }
            // 2.4+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_4)
            {
                ExtensionByte1 = reader.ReadByte();
                ExtensionByte2 = reader.ReadByte();
                SystemBiosMajorRelease = reader.ReadByte();
                SystemBiosMinorRelease = reader.ReadByte();
                EmbeddedFirmwareMajorRelease = reader.ReadByte();
                EmbeddedFirmwareMinorRelease = reader.ReadByte();
            }
        }
    }
}
