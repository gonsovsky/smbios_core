using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if NETCOREAPP
using System.Runtime.InteropServices;
#else
using System.Management;   
#endif

namespace SmBiosCore
{
    public class SmBiosParser
    {
#if NETCOREAPP
        protected void GetDmi()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                GetWinDmi();
            else
                GetUnixDmi();
        }

        [DllImport("kernel32.dll")]
        protected static extern uint GetSystemFirmwareTable(
             uint firmwareTableProviderSignature,
             uint firmwareTableId,
             [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] pFirmwareTableBuffer,
             uint bufferSize);

        protected void GetWinDmi()
        {
            byte[] byteSignature = { (byte)'B', (byte)'M', (byte)'S', (byte)'R' };
            var signature = BitConverter.ToUInt32(byteSignature, 0);
            uint size = GetSystemFirmwareTable(signature, 0, null, 0);
            Data = new byte[size];
            if (size != GetSystemFirmwareTable(signature, 0, Data, size))
                InvalidData();
            Version = Data[1] << 8 | Data[2];
            Data = Data.Skip(8).ToArray();
        }

        protected const string SmBiosFolder = "/sys/firmware/dmi/tables/";
        protected const string DmiFile = "DMI";
        protected const string EntryFile = "smbios_entry_point";

        protected void GetUnixDmi()
        {
            Data = File.ReadAllBytes(Path.Combine(SmBiosFolder, DmiFile));
            Head = File.ReadAllBytes(Path.Combine(SmBiosFolder, EntryFile));
            if (Head[0] == '_' && Head[1] == 'S' && Head[2] == 'M' && Head[3] == '_')
            {
                // VersionBI 2.x

                // entry point length
                if (Head[5] != 0x1F) InvalidData();
                // entry point revision
                if (Head[10] != 0) InvalidData();
                // intermediate anchor string
                if (Head[16] != '_' || Head[17] != 'D' || Head[18] != 'M' || Head[19] != 'I' || Head[20] != '_')
                    InvalidData();

                // get the SMBIOS VersionBI
                Version = Head[6] << 8 | Head[7];
            }
            else if (Head[0] == '_' && Head[1] == 'S' && Head[2] == 'M' && Head[3] == '3' && Head[4] == '_')
            {
                // VersionBI 3.x

                // entry point length
                if (Head[6] != 0x18) InvalidData();
                // entry point revision
                if (Head[10] != 0x01) InvalidData();

                // get the SMBIOS VersionBI
                Version = Head[7] << 8 | Head[8];
            }
            if (Version == 0) Version = SMBIOS_3_0;
            // is a valid VersionBI?
            if ((Version < SMBIOS_2_0 || Version > SMBIOS_2_8) && Version != SMBIOS_3_0)
                InvalidData();
        }
#else
        protected void GetDmi()
        {
            var scope = new ManagementScope("\\\\" + "." + "\\root\\WMI");
            scope.Connect();
            var wmiquery = new ObjectQuery("SELECT * FROM MSSmBios_RawSMBiosTables");
            var searcher = new ManagementObjectSearcher(scope, wmiquery);
            var coll = searcher.Get();
            var M_ByMajorVersion = 0;
            var M_ByMinorVersion = 0;
            foreach (var O in coll)
            {
                var queryObj = (ManagementObject) O;
                if (queryObj["SMBiosData"] != null) Data = (byte[])(queryObj["SMBiosData"]);
                if (queryObj["SmbiosMajorVersion"] != null) M_ByMajorVersion = (byte)(queryObj["SmbiosMajorVersion"]);
                if (queryObj["SmbiosMinorVersion"] != null) M_ByMinorVersion = (byte)(queryObj["SmbiosMinorVersion"]);
                //if (queryObj["Size"] != null) m_dwLen = (long)(queryObj["Size"]);
                //m_dwLen = m_pbBIOSData.Length;
            }
            Version = (ushort) (M_ByMajorVersion << 8 | M_ByMinorVersion);
            if (Version == 0)
                Version = SMBIOS_3_0;
        }
#endif

        public SmBiosParser()
        {
            Result = new SmBios();
            GetDmi();
            Traverse();
        }

        public class SmBios
        {
            public List<TypeBios> Bios = new List<TypeBios>();
            public List<TypeMemoryDevice> Memory = new List<TypeMemoryDevice>();
            public List<TypePhysicalMemory> PhyMemory = new List<TypePhysicalMemory>();
            public List<TypeBaseboard> BaseBoard = new List<TypeBaseboard>();
            public List<TypeProcessor> Processor = new List<TypeProcessor>();
        }

        public SmBios Result;

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

        protected const int DmiTypeBios = 0;
        protected const int DimTypeSysInfo = 1;
        protected const int DmiTypeBaseBoard = 2;
        protected const int DmiTypeSysEnclosure = 3;
        protected const int DmiTypeProcessor = 4;
        protected const int DmiTypeSysSlot = 9;
        protected const int DmiTypePhysmem = 16;
        protected const int DmiTypeMemory = 17;

        protected const int DmiEntryHeaderSize = 4;

        protected byte[] Data;
        protected byte[] Head;
        protected int Version;
        protected BinaryReader Reader;
        protected int Idx;

        protected void Traverse()
        {
            Idx = 0;
            Reader = new BinaryReader(new MemoryStream(Data));

            while (true)
            {
                var entry = Next();
                if (entry == null)
                    break;
                if (Reader.BaseStream.Position == Reader.BaseStream.Length)
                    break;
                if (entry.GetType() == typeof(TypeBios))
                    Result.Bios.Add(entry as TypeBios);
                if (entry.GetType() == typeof(TypeMemoryDevice))
                    Result.Memory.Add(entry as TypeMemoryDevice);
                if (entry.GetType() == typeof(TypePhysicalMemory))
                    Result.PhyMemory.Add(entry as TypePhysicalMemory);
                if (entry.GetType() == typeof(TypeBaseboard))
                    Result.BaseBoard.Add(entry as TypeBaseboard);
                if (entry.GetType() == typeof(TypeProcessor))
                    Result.Processor.Add(entry as TypeProcessor);
            }
            Reader.Dispose();
            Data = null;
        }

        protected Entry Next()
        {
            while (Idx < Data.Length)
            {
                var type = Data[Idx];
                if (type == 127)
                    return null;
                Entry result = null;
                if (type == DmiTypeBios)
                    result = new TypeBios();
                if (type == DmiTypeMemory)
                    result = new TypeMemoryDevice();
                if (type == DmiTypePhysmem)
                    result = new TypePhysicalMemory();
                if (type == DmiTypeBaseBoard)
                    result = new TypeBaseboard();
                if (type == DmiTypeProcessor)
                    result = new TypeProcessor();
                if (result == null)
                    result = new Entry(); //unknown
                result.Parse(this);
                return result;
            }
            return null;
        }

        protected void InvalidData()
        {
            throw new ApplicationException();
        }

        public class Entry
        {
            //private byte _type;
            private byte _length;
            //private short _handle;

            protected int VersionBi;
            protected BinaryReader Reader;
            protected string[] StringValues = { };

            protected string GetString(int b)
            {
                if (b <= 0) return "";
                if (b > StringValues.Length)
                    return "";
                return StringValues[b - 1];
            }

            public void Parse(SmBiosParser parser)
            {
                Reader = parser.Reader;
                VersionBi = parser.Version;
                //_type = parser.Data[parser.Idx];
                _length = parser.Data[parser.Idx + 1];
                //_handle = BitConverter.ToInt16(parser.Data, parser.Idx + 2);
                parser.Reader.BaseStream.Seek(parser.Idx + 4, SeekOrigin.Begin);
                byte[] P_BUnformattedSection;
                for (int j = parser.Idx + _length; ; j++)
                {
                    if ((parser.Data[j] == 0) && (parser.Data[j + 1] == 0))
                    {
                        P_BUnformattedSection = parser.Data.Skip(parser.Idx + _length).Take(j - parser.Idx - _length).ToArray();
                        parser.Idx = j + 2;
                        break;
                    }
                }
                StringValues = new string[] { };
                if (P_BUnformattedSection.Length > 0)
                    StringValues = Encoding.ASCII.GetString(P_BUnformattedSection).Split('\0');
                ParseBody();
                StringValues = new string[] { };
                parser.Reader.BaseStream.Seek(parser.Idx, SeekOrigin.Begin);
            }

            protected virtual void ParseBody() { }
        }

        #region BiosTables
        public class TypeBios : Entry
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

            protected override void ParseBody()
            {
                // 2.0+
                if (VersionBi >= SMBIOS_2_0)
                {
                    int Vendor_ = Reader.ReadByte();
                    int BiosVersion_ = Reader.ReadByte();
                    BiosStartingSegment = Reader.ReadUInt16();
                    int BiosReleaseDate_ = Reader.ReadByte();
                    BiosromSize = Reader.ReadByte();
                    for (var i = 0; i < 8; ++i)
                        BiosCharacteristics[i] = Reader.ReadByte();

                    Vendor = GetString(Vendor_);
                    BiosVersion = GetString(BiosVersion_);
                    BiosReleaseDate = GetString(BiosReleaseDate_);
                }
                // 2.4+
                if (VersionBi >= SMBIOS_2_4)
                {
                    ExtensionByte1 = Reader.ReadByte();
                    ExtensionByte2 = Reader.ReadByte();
                    SystemBiosMajorRelease = Reader.ReadByte();
                    SystemBiosMinorRelease = Reader.ReadByte();
                    EmbeddedFirmwareMajorRelease = Reader.ReadByte();
                    EmbeddedFirmwareMinorRelease = Reader.ReadByte();
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

            protected override void ParseBody()
            {
                // 2.1+
                if (VersionBi >= SMBIOS_2_1)
                {
                    PhysicalArrayHandle = Reader.ReadUInt16();
                    ErrorInformationHandle = Reader.ReadUInt16();
                    TotalWidth = Reader.ReadUInt16();
                    DataWidth = Reader.ReadUInt16();
                    Size = Reader.ReadUInt16();
                    FormFactor = Reader.ReadByte();
                    DeviceSet = Reader.ReadByte();
                    int DeviceLocator_ = Reader.ReadByte();
                    int BankLocator_ = Reader.ReadByte();
                    MemoryType = Reader.ReadByte();
                    TypeDetail = Reader.ReadUInt16();

                    DeviceLocator = GetString(DeviceLocator_);
                    BankLocator = GetString(BankLocator_);
                }
                // 2.3+
                if (VersionBi >= SMBIOS_2_3)
                {
                    Speed = Reader.ReadUInt16();
                    int Manufacturer_ = Reader.ReadByte();
                    int SerialNumber_ = Reader.ReadByte();
                    int AssetTagNumber_ = Reader.ReadByte();
                    int PartNumber_ = Reader.ReadByte();

                    Manufacturer = GetString(Manufacturer_);
                    SerialNumber = GetString(SerialNumber_);
                    AssetTagNumber = GetString(AssetTagNumber_);
                    PartNumber = GetString(PartNumber_);
                }
                // 2.6+
                if (VersionBi >= SMBIOS_2_6)
                {
                    Attributes = Reader.ReadByte();
                }
                // 2.7+
                if (VersionBi >= SMBIOS_2_7)
                {
                    ExtendedSize = Reader.ReadUInt32();
                    ConfiguredClockSpeed = Reader.ReadUInt16();
                }
                // 2.8+
                if (VersionBi >= SMBIOS_2_8)
                {
                    MinimumVoltage = Reader.ReadUInt16();
                    MinimumVoltage = Reader.ReadUInt16();
                    ConfiguredVoltage = Reader.ReadUInt16();
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
                if (VersionBi >= SMBIOS_2_1)
                {
                    Location = Reader.ReadByte();
                    Use = Reader.ReadByte();
                    ErrorCorrection = Reader.ReadByte();
                    MaximumCapacity = Reader.ReadUInt32();
                    ErrorInformationHandle = Reader.ReadUInt16();
                    NumberDevices = Reader.ReadUInt16();
                }
                // 2.7+
                if (VersionBi >= SMBIOS_2_7)
                {
                    ExtendedMaximumCapacity = Reader.ReadUInt64();
                }
            }
        }

        public class TypeBaseboard : Entry
        {
            public string Manufacturer;
            public string Product;
            public string Version;
            public string SerialNumber;       
            public string AssetTag;
            public byte FeatureFlags;
            public string LocationInChassis;
            public ushort ChassisHandle;

            #region BoardType
            public byte BoardType;

            public static Dictionary<byte, string> BoardTypeDictionary = new Dictionary<byte, string>()
            {
                {0x01, "Unknown"},
                {0x02, "Other"},
                {0x03, "Server Blade"},
                {0x04, "Connectivity Switch"},
                {0x05, "System Management Module"},
                {0x06, "Processor Module"},
                {0x07, "I/O Module"},
                {0x08, "Memory Module"},
                {0x09, "Daughter board"},
                {0x0A, "Motherboard (includes processor, memory, and I/O)"},
                {0x0B, "Processor/Memory Module"},
                {0x0C, "Processor/IO Module"},
                {0x0D, "Interconnect board"},
            };

            public string BoardTypeDisp =>
                BoardTypeDictionary.TryGetValue(BoardType, out var value)
                    ? value
                    : BoardType.ToString() + " :Table 15";
            #endregion

            //public byte NoOfContainedObjectHandles;
            //public ushort ContainedObjectHandles;

            protected override void ParseBody()
            {
                // 2.0+
                if (VersionBi >= SMBIOS_2_0)
                {
                    int Manufacturer_ = Reader.ReadByte();
                    int Product_ = Reader.ReadByte();
                    int Version_ = Reader.ReadByte();
                    int SerialNumber_ = Reader.ReadByte();
                    int AssetTag_ = Reader.ReadByte();
                    FeatureFlags = Reader.ReadByte();
                    int LocationInChassis_ = Reader.ReadByte();
                    ChassisHandle = Reader.ReadUInt16();
                    BoardType = Reader.ReadByte();
                    //NoOfContainedObjectHandles = reader.ReadByte();
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

            protected override void ParseBody()
            {
                // VersionBI 2.0
                if (VersionBi >= SMBIOS_2_0)
                {
                    int SocketDesignation_ = Reader.ReadByte();
                    ProcessorType = Reader.ReadByte();
                    ProcessorFamily = Reader.ReadByte();
                    int ProcessorManufacturer_ = Reader.ReadByte();
                    for (int i = 0; i < 8; ++i)
                        ProcessorId[i] = Reader.ReadByte();
                    int ProcessorVersion_ = Reader.ReadByte();
                    Voltage = Reader.ReadByte();
                    ExternalClock = Reader.ReadUInt16();
                    MaxSpeed = Reader.ReadUInt16();
                    CurrentSpeed = Reader.ReadUInt16();
                    Status = Reader.ReadByte();
                    ProcessorUpgrade = Reader.ReadByte();

                    SocketDesignation = GetString(SocketDesignation_);
                    ProcessorManufacturer = GetString(ProcessorManufacturer_);
                    ProcessorVersion = GetString(ProcessorVersion_);
                }
                if (VersionBi >= SMBIOS_2_1)
                {
                    L1CacheHandle = Reader.ReadUInt16();
                    L2CacheHandle = Reader.ReadUInt16();
                    L3CacheHandle = Reader.ReadUInt16();
                }
                if (VersionBi >= SMBIOS_2_3)
                {
                    int SerialNumber_ = Reader.ReadByte();
                    int AssetTagNumber_ = Reader.ReadByte();
                    int PartNumber_ = Reader.ReadByte();

                    SerialNumber = GetString(SerialNumber_);
                    AssetTagNumber = GetString(AssetTagNumber_);
                    PartNumber = GetString(PartNumber_);
                }
                if (VersionBi >= SMBIOS_2_5)
                {
                    CoreCount = Reader.ReadByte();
                    CoreEnabled = Reader.ReadByte();
                    ThreadCount = Reader.ReadByte();
                    ProcessorCharacteristics = Reader.ReadUInt16();
                }
                if (VersionBi >= SMBIOS_2_6)
                {
                    ProcessorFamily2 = Reader.ReadUInt16();
                }
                if (VersionBi >= SMBIOS_3_0)
                {
                    CoreCount2 = Reader.ReadUInt16();
                    CoreEnabled2 = Reader.ReadUInt16();
                    ThreadCount2 = Reader.ReadUInt16();
                }
            }
        }
        #endregion
    }
}