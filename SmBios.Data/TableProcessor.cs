using System.Collections.Generic;

namespace SmBios.Data
{
    public class TableProcessor : Table
    {
        public string SocketDesignation;
        public byte ProcessorType;

        #region ProcessorFamily
        public byte ProcessorFamily;

        public static Dictionary<byte, string> ProcessorFamilyDictionary = new Dictionary<byte, string>()
            {
                {205, "Intel® Core™ i5 processor"},
                {206, "Intel® Core™ i3 processor"},
                {207, "Intel® Core™ i9 processor"},
                {208, "Available for assignment"},
                {209, "Available for assignment"},
            };

        public string ProcessorFamilyDisp =>
            ProcessorFamilyDictionary.TryGetValue(ProcessorFamily, out var value)
                ? value
                : ProcessorFamily + " :Table 23";
        #endregion

        public string ProcessorManufacturer;
        public byte[] ProcessorId = new byte[8];
        public string ProcessorVersion;
        public byte Voltage;
        public ushort ExternalClock;
        public ushort MaxSpeed;
        public ushort CurrentSpeed;
        public byte Status;
        public byte ProcessorUpgrade;
        // 2.1+
        public ushort L1CacheHandle;
        public ushort L2CacheHandle;
        public ushort L3CacheHandle;
        // 2.3+
        public string SerialNumber;
        public string AssetTagNumber;
        public string PartNumber;
        // 2.5+
        public byte CoreCount;
        public byte CoreEnabled;
        public byte ThreadCount;
        public ushort ProcessorCharacteristics;
        // 2.6+
        public ushort ProcessorFamily2;
        // 3.0+
        public ushort CoreCount2;
        public ushort CoreEnabled2;
        public ushort ThreadCount2;
    }
}
