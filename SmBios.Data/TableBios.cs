using System.ComponentModel;

namespace SmBios.Data
{
    public class TableBios : Table
    {
        [DisplayName("Производитель")]
        public string Vendor { get; set; }

        [DisplayName("Версия")]
        public string BiosVersion { get; set; }

        [Browsable(false)]
        public ushort BiosStartingSegment { get; set; }

        [DisplayName("Дата выпуска")]
        public string BiosReleaseDate { get; set; }

        [Browsable(false)]
        public byte BiosromSize { get; set; }

        [Browsable(false)]
        public byte[] BiosCharacteristics { get; set; } = new byte[8];

        [Browsable(false)]
        public byte ExtensionByte1 { get; set; }

        [Browsable(false)]
        public byte ExtensionByte2 { get; set; }

        [Browsable(false)]
        public byte SystemBiosMajorRelease { get; set; }

        [Browsable(false)]
        public byte SystemBiosMinorRelease { get; set; }

        [Browsable(false)]
        public byte EmbeddedFirmwareMajorRelease { get; set; }

        [Browsable(false)]
        public byte EmbeddedFirmwareMinorRelease { get; set; }

        [Browsable(false)]
        public override string Name => BiosVersion;
    }
}
