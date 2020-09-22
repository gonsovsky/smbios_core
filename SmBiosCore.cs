using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SmBiosCore
{
    public class SmBiosCore
    {     
        protected static bool IsWin => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        protected const string SmBiosFolder = "/sys/firmware/dmi/tables/";
        protected const string DmiFile = "DMI";
        protected const string EntryFile = "smbios_entry_point";

        [DllImport("kernel32.dll")]
        protected static extern uint GetSystemFirmwareTable(
            uint FirmwareTableProviderSignature,
            uint FirmwareTableID,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] pFirmwareTableBuffer,
            uint BufferSize);

        protected byte[] GetWinDmi()
        {
            byte[] byteSignature = {(byte) 'B', (byte) 'M', (byte) 'S', (byte) 'R'};
            var signature = BitConverter.ToUInt32(byteSignature);
            uint size = GetSystemFirmwareTable(signature, 0, null, 0);
            var data = new byte[size];
            if (size != GetSystemFirmwareTable(signature, 0, data, size))
                InvalidData();
            return data;
        }

        protected static byte[] GetUnixDmi()
        {
            var p1 = File.ReadAllBytes(Path.Combine(SmBiosFolder, DmiFile));
            var p2 = File.ReadAllBytes(Path.Combine(SmBiosFolder, EntryFile));
            p1 = p2.Concat(new byte[]{0}).Concat(p1).ToArray();
            return p1;
        }

        public List<TypeBios> Bios = new List<TypeBios> {};
        public List<TypeMemoryDevice> Memory = new List<TypeMemoryDevice> { };
        public List<TypePhysicalMemory> PhyMemory = new List<TypePhysicalMemory> { };
        public List<TypeBaseboard> BaseBoard = new List<TypeBaseboard> { };
        public List<TypeProcessor> Processor = new List<TypeProcessor> { };

        public SmBiosCore()
        {
            if (IsWin)
                data = GetWinDmi();
            else
                data = GetUnixDmi();
            CheckBios();
            Traverse();
        }

        protected const int SMBIOS_2_0 = 0x0200;
        protected const int SMBIOS_2_1 = 0x0201;
        protected const int SMBIOS_2_2 = 0x0202;
        protected const int SMBIOS_2_3 = 0x0203;
        protected const int SMBIOS_2_4 = 0x0204;
        protected const int SMBIOS_2_5 = 0x0205;
        protected const int SMBIOS_2_6 = 0x0206;
        protected const int SMBIOS_2_7 = 0x0207;
        protected const int SMBIOS_2_8 = 0x0208;
        protected const int SMBIOS_3_0 = 0x0300;

        protected const int DMI_TYPE_BIOS = 0;
        protected const int DMI_TYPE_SYSINFO = 1;
        protected const int DMI_TYPE_BASEBOARD = 2;
        protected const int DMI_TYPE_SYSENCLOSURE = 3;
        protected const int DMI_TYPE_PROCESSOR = 4;
        protected const int DMI_TYPE_SYSSLOT = 9;
        protected const int DMI_TYPE_PHYSMEM = 16;
        protected const int DMI_TYPE_MEMORY = 17;

        public const int DMI_ENTRY_HEADER_SIZE = 4;

        private void CheckBios()
        {
            int vn = 0;
            if (IsWin == false)
            {
                if (data[0] == '_' && data[1] == 'S' && data[2] == 'M' && data[3] == '_')
                {
                    // version 2.x

                    // entry point length
                    if (data[5] != 0x1F) InvalidData();
                    // entry point revision
                    if (data[10] != 0) InvalidData();
                    // intermediate anchor string
                    if (data[16] != '_' || data[17] != 'D' || data[18] != 'M' || data[19] != 'I' || data[20] != '_')
                        InvalidData();

                    // get the SMBIOS version
                    vn = data[6] << 8 | data[7];
                }
                else if (data[0] == '_' && data[1] == 'S' && data[2] == 'M' && data[3] == '3' && data[4] == '_')
                {
                    // version 3.x

                    // entry point length
                    if (data[6] != 0x18) InvalidData();
                    // entry point revision
                    if (data[10] != 0x01) InvalidData();

                    // get the SMBIOS version
                    vn = data[7] << 8 | data[8];
                }
                else
                    InvalidData();
                data = data.Skip(32).ToArray();
            }
            else
            {
                vn = data[1] << 8 | data[2];
                data = data.Skip(8).ToArray();
            }

            if (_version == 0) _version = SMBIOS_3_0;
            if (_version > vn) _version = vn;
            // is a valid version?
            if ((_version < SMBIOS_2_0 || _version > SMBIOS_2_8) && _version != SMBIOS_3_0)
                InvalidData();
            return;
        }

        protected byte[] data;
        protected int _version;
        protected BinaryReader _reader;
        protected int i;

        protected void Traverse()
        {
            i = 0;
            _reader = new BinaryReader(new MemoryStream(data));
           
            Entry entry = null;
            while (true)
            {
                entry = Next();
                if (entry == null)
                    break;
                if (_reader.BaseStream.Position == _reader.BaseStream.Length)
                    break;
                if (entry.GetType() == typeof(TypeBios))
                    Bios.Add(entry as TypeBios);
                if (entry.GetType() == typeof(TypeMemoryDevice))
                    Memory.Add(entry as TypeMemoryDevice);
                if (entry.GetType() == typeof(TypePhysicalMemory))
                    PhyMemory.Add(entry as TypePhysicalMemory);
                if (entry.GetType() == typeof(TypeBaseboard))
                    BaseBoard.Add(entry as TypeBaseboard);
                if (entry.GetType() == typeof(TypeProcessor))
                    Processor.Add(entry as TypeProcessor);
            }
            _reader.Dispose();
            data = null;
        }

        private Entry Next()
        {
            while (i < data.Length)
            {
                var type = data[i];
                Entry result = null;
                if (type == DMI_TYPE_BIOS)
                    result = new TypeBios();
                if (type == DMI_TYPE_MEMORY)
                    result = new TypeMemoryDevice();
                if (type == DMI_TYPE_PHYSMEM)
                    result = new TypePhysicalMemory();
                if (type == DMI_TYPE_BASEBOARD)
                    result = new TypeBaseboard();
                if (type == DMI_TYPE_PROCESSOR)
                    result = new TypeProcessor();
                if (result == null)
                    result = new Entry(); //unknown
                    result.Parse(this);
                return result;
            }
            return null;
        }

        private void InvalidData()
        {
            throw new ApplicationException();
        }

        #region BiosTables
        public class TypeBios : Entry
        {
            internal int Vendor_;
            internal int BIOSVersion_;
            internal int BIOSReleaseDate_;

            public string Vendor;
            public string BIOSVersion;
            public ushort BIOSStartingSegment;
            public string BIOSReleaseDate;
            public byte BIOSROMSize;
            public byte[] BIOSCharacteristics = new byte[8];
            public byte ExtensionByte1;
            public byte ExtensionByte2;
            public byte SystemBIOSMajorRelease;
            public byte SystemBIOSMinorRelease;
            public byte EmbeddedFirmwareMajorRelease;
            public byte EmbeddedFirmwareMinorRelease;

            protected override void ParseBody()
            {
                // 2.0+
                if (_version >= SMBIOS_2_0)
                {
                    Vendor_ = _reader.ReadByte();
                    BIOSVersion_ = _reader.ReadByte(); ;
                    BIOSStartingSegment = _reader.ReadUInt16();
                    BIOSReleaseDate_ = _reader.ReadByte();
                    BIOSROMSize = _reader.ReadByte(); ;
                    for (var i = 0; i < 8; ++i)
                        BIOSCharacteristics[i] = _reader.ReadByte();

                    Vendor = GetString(Vendor_);
                    BIOSVersion = GetString(BIOSVersion_);
                    BIOSReleaseDate = GetString(BIOSReleaseDate_);
                }
                // 2.4+
                if (_version >= SMBIOS_2_4)
                {
                    ExtensionByte1 = _reader.ReadByte();
                    ExtensionByte2 = _reader.ReadByte();
                    SystemBIOSMajorRelease = _reader.ReadByte();
                    SystemBIOSMinorRelease = _reader.ReadByte();
                    EmbeddedFirmwareMajorRelease = _reader.ReadByte();
                    EmbeddedFirmwareMinorRelease = _reader.ReadByte();
                }
            }
        }

        public class TypeMemoryDevice : Entry
        {
            // 2.1+
            public ushort PhysicalArrayHandle;
            public ushort ErrorInformationHandle;
            public ushort TotalWidth;
            public ushort DataWidth;
            public ushort Size;
            public byte FormFactor;

            public byte DeviceSet;

            internal int DeviceLocator_;

            internal int BankLocator_;

            public string DeviceLocator;

            public string BankLocator;
            public byte MemoryType;
            public ushort TypeDetail;
            // 2.3+
            public ushort Speed;

            public string Manufacturer;

            public string SerialNumber;

            public string AssetTagNumber;

            public string PartNumber;
            // 2.6+
            public byte Attributes;
            // 2.7+
            public uint ExtendedSize;
            public ushort ConfiguredClockSpeed;
            // 2.8+
            public ushort MinimumVoltage;
            public ushort MaximumVoltage;
            public ushort ConfiguredVoltage;

            internal int Manufacturer_;

            internal int SerialNumber_;

            internal int AssetTagNumber_;

            internal int PartNumber_;

            protected override void ParseBody()
            {
                // 2.1+
                if (_version >= SMBIOS_2_1)
                {
                    PhysicalArrayHandle = _reader.ReadUInt16();
                    ErrorInformationHandle = _reader.ReadUInt16();
                    TotalWidth = _reader.ReadUInt16();
                    DataWidth = _reader.ReadUInt16();
                    Size = _reader.ReadUInt16();
                    FormFactor = _reader.ReadByte();
                    DeviceSet = _reader.ReadByte();
                    DeviceLocator_ = _reader.ReadByte();
                    BankLocator_ = _reader.ReadByte();
                    MemoryType = _reader.ReadByte();
                    TypeDetail = _reader.ReadUInt16();

                    DeviceLocator = GetString(DeviceLocator_);
                    BankLocator = GetString(BankLocator_);
                }
                // 2.3+
                if (_version >= SMBIOS_2_3)
                {
                    Speed = _reader.ReadUInt16();
                    Manufacturer_ = _reader.ReadByte();
                    SerialNumber_ = _reader.ReadByte();
                    AssetTagNumber_ = _reader.ReadByte();
                    PartNumber_ = _reader.ReadByte();

                    Manufacturer = GetString(Manufacturer_);
                    SerialNumber = GetString(SerialNumber_);
                    AssetTagNumber = GetString(AssetTagNumber_);
                    PartNumber = GetString(PartNumber_);
                }
                // 2.6+
                if (_version >= SMBIOS_2_6)
                {
                    Attributes = _reader.ReadByte();
                }
                // 2.7+
                if (_version >= SMBIOS_2_7)
                {
                    ExtendedSize = _reader.ReadUInt32();
                    ConfiguredClockSpeed = _reader.ReadUInt16();
                }
                // 2.8+
                if (_version >= SMBIOS_2_8)
                {
                    MinimumVoltage = _reader.ReadUInt16();
                    MinimumVoltage = _reader.ReadUInt16();
                    ConfiguredVoltage = _reader.ReadUInt16();
                }
            }
        }

        public class TypePhysicalMemory : Entry
        {
            // 2.1+
            public byte Location;
            public byte Use;
            public byte ErrorCorrection;
            public uint MaximumCapacity;
            public ushort ErrorInformationHandle;
            public ushort NumberDevices;
            // 2.7+
            public ulong ExtendedMaximumCapacity;

            protected override void ParseBody()
            {
                if (_version >= SMBIOS_2_1)
                {
                    Location = _reader.ReadByte();
                    Use = _reader.ReadByte();
                    ErrorCorrection = _reader.ReadByte();
                    MaximumCapacity = _reader.ReadUInt32();
                    ErrorInformationHandle = _reader.ReadUInt16();
                    NumberDevices = _reader.ReadUInt16();
                }
                // 2.7+
                if (_version >= SMBIOS_2_7)
                {
                    ExtendedMaximumCapacity = _reader.ReadUInt64();
                }
            }
        }

        public class TypeBaseboard : Entry
        {
            internal int Manufacturer_;
            internal int Product_;
            internal int Version_;
            internal int SerialNumber_;
            internal int AssetTag_;

            public string Manufacturer;
            public string Product;
            public string Version;
            public string SerialNumber;

            public string AssetTag;
            public byte FeatureFlags;

            internal int LocationInChassis_;

            public string LocationInChassis;
            public ushort ChassisHandle;
            public byte BoardType;
            public byte NoOfContainedObjectHandles;

            public ushort ContainedObjectHandles;

            protected override void ParseBody()
            {
                // 2.0+
                if (_version >= SMBIOS_2_0)
                {
                    Manufacturer_ = _reader.ReadByte();
                    Product_ = _reader.ReadByte();
                    Version_ = _reader.ReadByte();
                    SerialNumber_ = _reader.ReadByte();
                    AssetTag_ = _reader.ReadByte();
                    FeatureFlags = _reader.ReadByte();
                    LocationInChassis_ = _reader.ReadByte();
                    ChassisHandle = _reader.ReadUInt16();
                    BoardType = _reader.ReadByte();
                    NoOfContainedObjectHandles = _reader.ReadByte();
                    //ContainedObjectHandles = (uint16_t*)ptr_;
                    //ptr_ += entry_.data.baseboard.NoOfContainedObjectHandles * sizeof(uint16_t);
                    Manufacturer = GetString(Manufacturer_);
                    Product = GetString(Product_);
                    Version = GetString(Version_);
                    SerialNumber = GetString(SerialNumber_);
                    AssetTag = GetString(AssetTag_);
                    LocationInChassis = GetString(LocationInChassis_);
                }
            }
        }

        public class TypeProcessor : Entry
        {
            internal int SocketDesignation_;
            public string SocketDesignation;
            public byte ProcessorType;
            public byte ProcessorFamily;
            internal int ProcessorManufacturer_;
            public string ProcessorManufacturer;
            public byte[] ProcessorID = new byte[8];
            internal int ProcessorVersion_;
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

            internal int SerialNumber_;
            internal int AssetTagNumber_;
            internal int PartNumber_;

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

            protected override void ParseBody()
            {
                // version 2.0
                if (_version >= SMBIOS_2_0)
                {
                    SocketDesignation_ = _reader.ReadByte();
                    ProcessorType = _reader.ReadByte();
                    ProcessorFamily = _reader.ReadByte();
                    ProcessorManufacturer_ = _reader.ReadByte();
                    for (int i = 0; i < 8; ++i)
                        ProcessorID[i] = _reader.ReadByte();
                    ProcessorVersion_ = _reader.ReadByte();
                    Voltage = _reader.ReadByte();
                    ExternalClock = _reader.ReadUInt16();
                    MaxSpeed = _reader.ReadUInt16();
                    CurrentSpeed = _reader.ReadUInt16();
                    Status = _reader.ReadByte();
                    ProcessorUpgrade = _reader.ReadByte();

                    SocketDesignation = GetString(SocketDesignation_);
                    ProcessorManufacturer = GetString(ProcessorManufacturer_);
                    ProcessorVersion = GetString(ProcessorVersion_);
                }
                if (_version >= SMBIOS_2_1)
                {
                    L1CacheHandle = _reader.ReadUInt16();
                    L2CacheHandle = _reader.ReadUInt16();
                    L3CacheHandle = _reader.ReadUInt16();
                }
                if (_version >= SMBIOS_2_3)
                {
                    SerialNumber_ = _reader.ReadByte();
                    AssetTagNumber_ = _reader.ReadByte();
                    PartNumber_ = _reader.ReadByte();

                    SerialNumber = GetString(SerialNumber_);
                    AssetTagNumber = GetString(AssetTagNumber_);
                    PartNumber = GetString(PartNumber_);
                }
                if (_version >= SMBIOS_2_5)
                {
                    CoreCount = _reader.ReadByte();
                    CoreEnabled = _reader.ReadByte();
                    ThreadCount = _reader.ReadByte();
                    ProcessorCharacteristics = _reader.ReadUInt16();
                }
                if (_version >= SMBIOS_2_6)
                {
                    ProcessorFamily2 = _reader.ReadUInt16();
                }
                if (_version >= SMBIOS_3_0)
                {
                    CoreCount2 = _reader.ReadUInt16();
                    CoreEnabled2 = _reader.ReadUInt16();
                    ThreadCount2 = _reader.ReadUInt16();
                }
            }
        }

        public class Entry
        {
            internal byte type;
            internal byte length;
            internal short handle;

            protected int _version;
            protected BinaryReader _reader;
            protected string[] strs = new string[] { };

            protected string GetString(int b)
            {
                if (b <= 0) return "";
                if (b > strs.Length)
                    return "";
                return strs[b - 1];
            }

            public void Parse(SmBiosCore parser)
            {
                _reader = parser._reader;
                _version = parser._version;
                type = parser.data[parser.i];
                length = parser.data[parser.i + 1];
                handle = BitConverter.ToInt16(parser.data, parser.i + 2);
                parser._reader.BaseStream.Seek(parser.i + 4, SeekOrigin.Begin);
                byte[] p_bFormattedSection;
                byte[] p_bUnformattedSection;
                p_bFormattedSection = parser.data.Skip(parser.i).Take(length).ToArray();
                for (int j = parser.i + length; ; j++)
                {
                    if ((parser.data[j] == 0) && (parser.data[j + 1] == 0))
                    {
                        p_bUnformattedSection = parser.data.Skip(parser.i + length).Take(j - parser.i - length).ToArray();
                        parser.i = j + 2;
                        break;
                    }
                }
                strs = new string[] { };
                if (p_bUnformattedSection.Length > 0)
                    strs = Encoding.ASCII.GetString(p_bUnformattedSection).Split('\0');
                ParseBody();
                strs = new string[] { };
                parser._reader.BaseStream.Seek(parser.i, SeekOrigin.Begin);
            }

            protected virtual void ParseBody() { }
        }
        #endregion
    }
}
