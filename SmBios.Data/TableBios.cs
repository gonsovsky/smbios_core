namespace SmBios.Data
{
    public class TableBios : Table
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
    }
}
