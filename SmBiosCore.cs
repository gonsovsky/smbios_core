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
    public class SmBiosCore
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
             uint FirmwareTableProviderSignature,
             uint FirmwareTableID,
             [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] pFirmwareTableBuffer,
             uint BufferSize);

        protected void GetWinDmi()
        {
            byte[] byteSignature = { (byte)'B', (byte)'M', (byte)'S', (byte)'R' };
            var signature = BitConverter.ToUInt32(byteSignature, 0);
            uint size = GetSystemFirmwareTable(signature, 0, null, 0);
            data = new byte[size];
            if (size != GetSystemFirmwareTable(signature, 0, data, size))
                InvalidData();
            version = data[1] << 8 | data[2];
            data = data.Skip(8).ToArray();
        }

        protected const string SmBiosFolder = "/sys/firmware/dmi/tables/";
        protected const string DmiFile = "DMI";
        protected const string EntryFile = "smbios_entry_point";

        protected void GetUnixDmi()
        {
            data = File.ReadAllBytes(Path.Combine(SmBiosFolder, DmiFile));
            head = File.ReadAllBytes(Path.Combine(SmBiosFolder, EntryFile));
            if (head[0] == '_' && head[1] == 'S' && head[2] == 'M' && head[3] == '_')
            {
                // version 2.x

                // entry point length
                if (head[5] != 0x1F) InvalidData();
                // entry point revision
                if (head[10] != 0) InvalidData();
                // intermediate anchor string
                if (head[16] != '_' || head[17] != 'D' || head[18] != 'M' || head[19] != 'I' || head[20] != '_')
                    InvalidData();

                // get the SMBIOS version
                version = head[6] << 8 | head[7];
            }
            else if (head[0] == '_' && head[1] == 'S' && head[2] == 'M' && head[3] == '3' && head[4] == '_')
            {
                // version 3.x

                // entry point length
                if (head[6] != 0x18) InvalidData();
                // entry point revision
                if (head[10] != 0x01) InvalidData();

                // get the SMBIOS version
                version = head[7] << 8 | head[8];
            }
            if (version == 0) version = SMBIOS_3_0;
            // is a valid version?
            if ((version < SMBIOS_2_0 || version > SMBIOS_2_8) && version != SMBIOS_3_0)
                InvalidData();
        }
#else
        protected void GetDmi()
        {
            ManagementScope scope = new ManagementScope("\\\\" + "." + "\\root\\WMI");
            scope.Connect();
            ObjectQuery wmiquery = new ObjectQuery("SELECT * FROM MSSmBios_RawSMBiosTables");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, wmiquery);
            ManagementObjectCollection coll = searcher.Get();
            foreach (ManagementObject queryObj in coll)
            {
                if (queryObj["SMBiosData"] != null) data = (byte[])(queryObj["SMBiosData"]);
                //if (queryObj["SmbiosMajorVersion"] != null) m_byMajorVersion = (byte)(queryObj["SmbiosMajorVersion"]);
                //if (queryObj["SmbiosMinorVersion"] != null) m_byMinorVersion = (byte)(queryObj["SmbiosMinorVersion"]);
                //if (queryObj["Size"] != null) m_dwLen = (long)(queryObj["Size"]);
                //m_dwLen = m_pbBIOSData.Length;
            }
            if (version == 0)
                version = SMBIOS_3_0;
        }
#endif

        public SmBiosCore()
        {
            GetDmi();
            Traverse();
        }

        public List<TypeBios> Bios = new List<TypeBios> { };
        public List<TypeMemoryDevice> Memory = new List<TypeMemoryDevice> { };
        public List<TypePhysicalMemory> PhyMemory = new List<TypePhysicalMemory> { };
        public List<TypeBaseboard> BaseBoard = new List<TypeBaseboard> { };
        public List<TypeProcessor> Processor = new List<TypeProcessor> { };

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

        protected const int DMI_ENTRY_HEADER_SIZE = 4;

        protected byte[] data;
        protected byte[] head;
        protected int version;
        protected BinaryReader reader;
        protected int i;

        protected void Traverse()
        {
            i = 0;
            reader = new BinaryReader(new MemoryStream(data));

            Entry entry = null;
            while (true)
            {
                entry = Next();
                if (entry == null)
                    break;
                if (reader.BaseStream.Position == reader.BaseStream.Length)
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
            reader.Dispose();
            data = null;
        }

        protected Entry Next()
        {
            while (i < data.Length)
            {
                var type = data[i];
                if (type == 127)
                    return null;
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

        protected void InvalidData()
        {
            throw new ApplicationException();
        }

        #region BiosTables
        public class TypeBios : Entry
        {
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
                int Vendor_;
                int BIOSVersion_;
                int BIOSReleaseDate_;

                // 2.0+
                if (version >= SMBIOS_2_0)
                {
                    Vendor_ = reader.ReadByte();
                    BIOSVersion_ = reader.ReadByte(); ;
                    BIOSStartingSegment = reader.ReadUInt16();
                    BIOSReleaseDate_ = reader.ReadByte();
                    BIOSROMSize = reader.ReadByte(); ;
                    for (var i = 0; i < 8; ++i)
                        BIOSCharacteristics[i] = reader.ReadByte();

                    Vendor = GetString(Vendor_);
                    BIOSVersion = GetString(BIOSVersion_);
                    BIOSReleaseDate = GetString(BIOSReleaseDate_);
                }
                // 2.4+
                if (version >= SMBIOS_2_4)
                {
                    ExtensionByte1 = reader.ReadByte();
                    ExtensionByte2 = reader.ReadByte();
                    SystemBIOSMajorRelease = reader.ReadByte();
                    SystemBIOSMinorRelease = reader.ReadByte();
                    EmbeddedFirmwareMajorRelease = reader.ReadByte();
                    EmbeddedFirmwareMinorRelease = reader.ReadByte();
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
                int DeviceLocator_;          
                int BankLocator_;
                int Manufacturer_;
                int SerialNumber_;
                int AssetTagNumber_;
                int PartNumber_;

                // 2.1+
                if (version >= SMBIOS_2_1)
                {
                    PhysicalArrayHandle = reader.ReadUInt16();
                    ErrorInformationHandle = reader.ReadUInt16();
                    TotalWidth = reader.ReadUInt16();
                    DataWidth = reader.ReadUInt16();
                    Size = reader.ReadUInt16();
                    FormFactor = reader.ReadByte();
                    DeviceSet = reader.ReadByte();
                    DeviceLocator_ = reader.ReadByte();
                    BankLocator_ = reader.ReadByte();
                    MemoryType = reader.ReadByte();
                    TypeDetail = reader.ReadUInt16();

                    DeviceLocator = GetString(DeviceLocator_);
                    BankLocator = GetString(BankLocator_);
                }
                // 2.3+
                if (version >= SMBIOS_2_3)
                {
                    Speed = reader.ReadUInt16();
                    Manufacturer_ = reader.ReadByte();
                    SerialNumber_ = reader.ReadByte();
                    AssetTagNumber_ = reader.ReadByte();
                    PartNumber_ = reader.ReadByte();

                    Manufacturer = GetString(Manufacturer_);
                    SerialNumber = GetString(SerialNumber_);
                    AssetTagNumber = GetString(AssetTagNumber_);
                    PartNumber = GetString(PartNumber_);
                }
                // 2.6+
                if (version >= SMBIOS_2_6)
                {
                    Attributes = reader.ReadByte();
                }
                // 2.7+
                if (version >= SMBIOS_2_7)
                {
                    ExtendedSize = reader.ReadUInt32();
                    ConfiguredClockSpeed = reader.ReadUInt16();
                }
                // 2.8+
                if (version >= SMBIOS_2_8)
                {
                    MinimumVoltage = reader.ReadUInt16();
                    MinimumVoltage = reader.ReadUInt16();
                    ConfiguredVoltage = reader.ReadUInt16();
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
                if (version >= SMBIOS_2_1)
                {
                    Location = reader.ReadByte();
                    Use = reader.ReadByte();
                    ErrorCorrection = reader.ReadByte();
                    MaximumCapacity = reader.ReadUInt32();
                    ErrorInformationHandle = reader.ReadUInt16();
                    NumberDevices = reader.ReadUInt16();
                }
                // 2.7+
                if (version >= SMBIOS_2_7)
                {
                    ExtendedMaximumCapacity = reader.ReadUInt64();
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
            public byte BoardType;
            //public byte NoOfContainedObjectHandles;
            //public ushort ContainedObjectHandles;

            protected override void ParseBody()
            {
                int Manufacturer_;
                int Product_;
                int Version_;
                int SerialNumber_;
                int AssetTag_;
                int LocationInChassis_;

                // 2.0+
                if (version >= SMBIOS_2_0)
                {
                    Manufacturer_ = reader.ReadByte();
                    Product_ = reader.ReadByte();
                    Version_ = reader.ReadByte();
                    SerialNumber_ = reader.ReadByte();
                    AssetTag_ = reader.ReadByte();
                    FeatureFlags = reader.ReadByte();
                    LocationInChassis_ = reader.ReadByte();
                    ChassisHandle = reader.ReadUInt16();
                    BoardType = reader.ReadByte();
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
            public byte ProcessorFamily;
            public string ProcessorManufacturer;
            public byte[] ProcessorID = new byte[8];
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
                int ProcessorVersion_;
                int SocketDesignation_;
                int ProcessorManufacturer_;
                int SerialNumber_;
                int AssetTagNumber_;
                int PartNumber_;

                // version 2.0
                if (version >= SMBIOS_2_0)
                {
                    SocketDesignation_ = reader.ReadByte();
                    ProcessorType = reader.ReadByte();
                    ProcessorFamily = reader.ReadByte();
                    ProcessorManufacturer_ = reader.ReadByte();
                    for (int i = 0; i < 8; ++i)
                        ProcessorID[i] = reader.ReadByte();
                    ProcessorVersion_ = reader.ReadByte();
                    Voltage = reader.ReadByte();
                    ExternalClock = reader.ReadUInt16();
                    MaxSpeed = reader.ReadUInt16();
                    CurrentSpeed = reader.ReadUInt16();
                    Status = reader.ReadByte();
                    ProcessorUpgrade = reader.ReadByte();

                    SocketDesignation = GetString(SocketDesignation_);
                    ProcessorManufacturer = GetString(ProcessorManufacturer_);
                    ProcessorVersion = GetString(ProcessorVersion_);
                }
                if (version >= SMBIOS_2_1)
                {
                    L1CacheHandle = reader.ReadUInt16();
                    L2CacheHandle = reader.ReadUInt16();
                    L3CacheHandle = reader.ReadUInt16();
                }
                if (version >= SMBIOS_2_3)
                {
                    SerialNumber_ = reader.ReadByte();
                    AssetTagNumber_ = reader.ReadByte();
                    PartNumber_ = reader.ReadByte();

                    SerialNumber = GetString(SerialNumber_);
                    AssetTagNumber = GetString(AssetTagNumber_);
                    PartNumber = GetString(PartNumber_);
                }
                if (version >= SMBIOS_2_5)
                {
                    CoreCount = reader.ReadByte();
                    CoreEnabled = reader.ReadByte();
                    ThreadCount = reader.ReadByte();
                    ProcessorCharacteristics = reader.ReadUInt16();
                }
                if (version >= SMBIOS_2_6)
                {
                    ProcessorFamily2 = reader.ReadUInt16();
                }
                if (version >= SMBIOS_3_0)
                {
                    CoreCount2 = reader.ReadUInt16();
                    CoreEnabled2 = reader.ReadUInt16();
                    ThreadCount2 = reader.ReadUInt16();
                }
            }
        }

        public class Entry
        {
            private byte type;
            private byte length;
            private short handle;

            protected int version;
            protected BinaryReader reader;
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
                reader = parser.reader;
                version = parser.version;
                type = parser.data[parser.i];
                length = parser.data[parser.i + 1];
                handle = BitConverter.ToInt16(parser.data, parser.i + 2);
                parser.reader.BaseStream.Seek(parser.i + 4, SeekOrigin.Begin);
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
                parser.reader.BaseStream.Seek(parser.i, SeekOrigin.Begin);
            }

            protected virtual void ParseBody() { }
        }
        #endregion
    }
}