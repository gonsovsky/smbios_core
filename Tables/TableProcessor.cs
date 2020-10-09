using System.Collections.Generic;
using System.IO;

namespace SmBiosCore.Tables
{
    public class TableProcessor : SmBiosTable
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
                : ProcessorFamily.ToString() + " :Table 23";
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

        internal TableProcessor(BinaryReader reader, SmBiosTableHeader header)
        {
            // VersionBI 2.0
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_0)
            {
                int SocketDesignation_ = reader.ReadByte();
                ProcessorType = reader.ReadByte();
                ProcessorFamily = reader.ReadByte();
                int ProcessorManufacturer_ = reader.ReadByte();
                for (int i = 0; i < 8; ++i)
                    ProcessorId[i] = reader.ReadByte();
                int ProcessorVersion_ = reader.ReadByte();
                Voltage = reader.ReadByte();
                ExternalClock = reader.ReadUInt16();
                MaxSpeed = reader.ReadUInt16();
                CurrentSpeed = reader.ReadUInt16();
                Status = reader.ReadByte();
                ProcessorUpgrade = reader.ReadByte();

                SocketDesignation = header.GetString(SocketDesignation_);
                ProcessorManufacturer = header.GetString(ProcessorManufacturer_);
                ProcessorVersion = header.GetString(ProcessorVersion_);
            }
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_1)
            {
                L1CacheHandle = reader.ReadUInt16();
                L2CacheHandle = reader.ReadUInt16();
                L3CacheHandle = reader.ReadUInt16();
            }
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_3)
            {
                int SerialNumber_ = reader.ReadByte();
                int AssetTagNumber_ = reader.ReadByte();
                int PartNumber_ = reader.ReadByte();

                SerialNumber = header.GetString(SerialNumber_);
                AssetTagNumber = header.GetString(AssetTagNumber_);
                PartNumber = header.GetString(PartNumber_);
            }
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_5)
            {
                CoreCount = reader.ReadByte();
                CoreEnabled = reader.ReadByte();
                ThreadCount = reader.ReadByte();
                ProcessorCharacteristics = reader.ReadUInt16();
            }
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_6)
            {
                ProcessorFamily2 = reader.ReadUInt16();
            }
            if (header.VersionBi >= SmBiosConst.SMBIOS_3_0)
            {
                CoreCount2 = reader.ReadUInt16();
                CoreEnabled2 = reader.ReadUInt16();
                ThreadCount2 = reader.ReadUInt16();
            }
        }
    }
}
