using System.Collections.Generic;
using System.ComponentModel;

namespace SmBios.Data
{
    public class TablePhysicalMemory : Table
    {
        // 2.1+

        #region Location
        [Browsable(false)]
        public byte Location { get; set; }

        public static Dictionary<byte, string> LocationDictionary = new Dictionary<byte, string>()
        {
            {0x01, "Unknown"},
            {0x02, "Other"},
            {0x03, "System board or motherboard"},
            {0x04, "ISA add-on card"},
            {0x05, "EISA add-on card"},
            {0x06, "PCI add-on card"},
            {0x07, "MCA add-on card"},
            {0x08, "PCMCIA add-on card"},
            {0x09, "Proprietary add-on card"}
        };

        [DisplayName("Размещение")]
        public string LocationDisp =>
            LocationDictionary.TryGetValue(Location, out var value)
                ? value
                : Location.ToString() + " :Table 71";
        #endregion

        [Browsable(false)]
        public byte Use { get; set; }

        #region ErrorCorrection
        [Browsable(false)]
        public byte ErrorCorrection { get; set; }

        public static Dictionary<byte, string> ErrorCorrectionDictionary = new Dictionary<byte, string>()
        {
            {0x01, "Other"},
            {0x02, "Unknown"},
            {0x03, "None"},
            {0x04, "Parity"},
            {0x05, "Single-bit ECC"},
            {0x06, "Multi-bit ECC"},
            {0x07, "CRC"}
        };

        [DisplayName("Коррекция ошибок")]
        public string ErrorCorrectionDisp =>
            ErrorCorrectionDictionary.TryGetValue(Location, out var value)
                ? value
                : ErrorCorrection.ToString() + " :Table 73";
        #endregion

        [Browsable(false)]
        public uint MaximumCapacity { get; set; }

        [DisplayName("Максимальный объем")]
        [Browsable(true)]
        public string MaximumCapacityDisp =>
            $"{MaximumCapacity / 1024 / 1024} ГБ";

        [Browsable(false)]
        public ushort ErrorInformationHandle { get; set; }

        [DisplayName("Кол-во устройств")]
        public ushort NumberDevices { get; set; }

        // 2.7+
        [Browsable(false)]
        public ulong ExtendedMaximumCapacity { get; set; }

        [Browsable(false)]
        public override string Name => LocationDisp;
    }

}
