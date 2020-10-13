using System.Collections.Generic;
using System.ComponentModel;

namespace SmBios.Data
{
    public class TableProcessor : Table
    {
        [DisplayName("Тип разъема")]
        public string SocketDesignation { get; set; }

        [Browsable(false)]
        public byte ProcessorType { get; set; }

        [DisplayName("Тип")]
        [Browsable(true)]
        public string ProcessorTypeDisp
        {
            get
            {
                string[] type = {   "Other", /* 0x01 */
                    "Unknown",
                    "Central Processor",
                    "Math Processor",
                    "DSP Processor",
                    "Video Processor" /* 0x06 */
                };
                if (ProcessorType >= 0x01 && ProcessorType <= 0x06)
                    return type[ProcessorType - 0x01];
                return "(нет в спецификации)";
            }
        }

        #region ProcessorFamily
        [Browsable(false)]
        public byte ProcessorFamily { get; set; }

        public static Dictionary<byte, string> ProcessorFamilyDictionary = new Dictionary<byte, string>()
            {
                {205, "Intel® Core™ i5 processor"},
                {206, "Intel® Core™ i3 processor"},
                {207, "Intel® Core™ i9 processor"},
                {208, "Available for assignment"},
                {209, "Available for assignment"},
            };

        [DisplayName("Семейство")]
        public string ProcessorFamilyDisp =>
            ProcessorFamilyDictionary.TryGetValue(ProcessorFamily, out var value)
                ? value
                : ProcessorFamily + " :Table 23";
        #endregion

        [DisplayName("Производитель")]
        public string ProcessorManufacturer { get; set; }

        [Browsable(false)]
        public byte[] ProcessorId { get; set; } = new byte[8];

        [DisplayName("Версия")]
        public string ProcessorVersion { get; set; }

        [Browsable(false)]
        public byte Voltage { get; set; }

        [DisplayName("Напряжение питания")]
        public string VolatageDisp
        {
            get
            {
                /* 7.5.4 */
                string[] voltage =
                {
                    "5.0 V", /* 0 */
                    "3.3 V",
                    "2.9 V" /* 2 */
                };
                int i;
                string result = "";

                if ((Voltage & (1 << 7)) != 0)
                    result += (float) (Voltage & 0x7f) / 10 + " V";
                else
                {
                    for (i = 0; i <= 2; i++)
                        if ((Voltage & (1 << i)) != 0)
                            result += voltage[i];
                    if (Voltage == 0x00)
                        result = " Unknown";
                }

                return result;
            }
        }

        [Browsable(false)]
        public ushort ExternalClock { get; set; }

        [DisplayName("Частота системной шины")]
        public string ExternalClockDisp => $"{ExternalClock} МГц";

        [Browsable(false)]
        public ushort MaxSpeed { get; set; }

        [DisplayName("Максимальная частота")]
        public string MaxSpeedDisp => $"{MaxSpeed} МГц";

        [Browsable(false)]
        public ushort CurrentSpeed { get; set; }

        [DisplayName("Текущая частота")]
        public string CurrentSpeedDisp => $"{CurrentSpeed} МГц";

        public byte Status { get; set; }

        public byte ProcessorUpgrade;
        // 2.1+

        [Browsable(false)]
        public ushort L1CacheHandle { get; set; }

        [Browsable(false)]
        public ushort L2CacheHandle { get; set; }

        [Browsable(false)]
        public ushort L3CacheHandle { get; set; }
        // 2.3+

        [DisplayName("Серийный номер")]
        public string SerialNumber { get; set; }

        [DisplayName("Тег имущества")]
        public string AssetTagNumber { get; set; }

        [Browsable(false)]
        public string PartNumber { get; set; }
        // 2.5+

        [DisplayName("Кол-во ядер")]
        public byte CoreCount { get; set; }

        [Browsable(false)]
        public byte CoreEnabled { get; set; }

        [Browsable(false)]
        public byte ThreadCount { get; set; }

        [Browsable(false)]
        public ushort ProcessorCharacteristics;
        // 2.6+

        [Browsable(false)]
        public ushort ProcessorFamily2 { get; set; }
        // 3.0+

        [Browsable(false)]
        public ushort CoreCount2 { get; set; }

        [Browsable(false)]
        public ushort CoreEnabled2 { get; set; }

        [Browsable(false)]
        public ushort ThreadCount2 { get; set; }

        [Browsable(false)]
        public override string Name => ProcessorVersion;
    }
}
