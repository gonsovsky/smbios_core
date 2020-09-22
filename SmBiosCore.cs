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
        protected static bool IsWin => Environment.OSVersion.Platform == PlatformID.Win32NT;

        protected const string SmBiosFolder = "D:/sys/firmware/dmi/tables/";
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

        public class TypeBios: Entry
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
        }

        public class TypeMemoryDevice: Entry
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
        }

        public class TypePhysicalMemory: Entry
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
        }

        public class TypeBaseboard: Entry
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
        }

        public class TypeProcessor: Entry
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
        }


        public class Entry
        {
            internal byte type;
            internal byte length;
            internal short handle;

            internal byte[] p_bFormattedSection;
            internal byte[] p_bUnformattedSection;
        }

        protected static byte[] GetUnixDmi()
        {
            var p1 = File.ReadAllBytes(SmBiosFolder + DmiFile);
            var p2 = File.ReadAllBytes(SmBiosFolder + EntryFile);
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
            reader.Dispose();
            data = null;
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
            }
            else
            {
                vn = data[1] << 8 | data[2];
                data = data.Skip(8).ToArray();
            }

            if (version_ == 0) version_ = SMBIOS_3_0;
            if (version_ > vn) version_ = vn;
            // is a valid version?
            if ((version_ < SMBIOS_2_0 || version_ > SMBIOS_2_8) && version_ != SMBIOS_3_0)
                InvalidData();
            //Reset();
            return;

        }

        protected byte[] data;
        protected int version_;
        protected string[] strs;
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
        }

        private Entry Next()
        {
            while (i < data.Length)
            {
                var type = data[i];
                Entry result = new Entry();
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
                result.type = type;
                result.length = data[i + 1];
                result.handle = BitConverter.ToInt16(data, i + 2);
                reader.BaseStream.Seek(i+4, SeekOrigin.Begin);
                result.p_bFormattedSection = data.Skip(i).Take(result.length).ToArray();
                for (int j = i + result.length; ; j++)
                {
                    if ((data[j] == 0) && (data[j + 1] == 0))
                    {
                        result.p_bUnformattedSection = data.Skip(i + result.length).Take(j - i - result.length).ToArray();
                        i = j + 2;
                        break;
                    }
                }
                strs = new string[] { };
                if (result.p_bUnformattedSection.Length > 0) 
                    strs = Encoding.ASCII.GetString(result.p_bUnformattedSection).Split('\0');
                try
                {
                    parse(result);
                }
                catch (Exception e)
                {
                    result = new Entry();
                }
                reader.BaseStream.Seek(i, SeekOrigin.Begin);
                return result;
            }
            return null;
        }

        private string getString(int b)
        {
            if (b <= 0) return "";
            if (b > strs.Length)
                return "";
            return strs[b-1];
        }

        private void parse(Entry entry_)
        {
            if (entry_.type == DMI_TYPE_BIOS)
            {
                var bios = entry_ as TypeBios;
                // 2.0+
                if (version_ >= SMBIOS_2_0)
                {
                    bios.Vendor_ = reader.ReadByte();
                    bios.BIOSVersion_ = reader.ReadByte(); ;
                    bios.BIOSStartingSegment = reader.ReadUInt16();
                    bios.BIOSReleaseDate_ = reader.ReadByte();
                    bios.BIOSROMSize = reader.ReadByte(); ;
                    for (var i = 0; i < 8; ++i)
                        bios.BIOSCharacteristics[i] = reader.ReadByte();

                    bios.Vendor = getString(bios.Vendor_);
                    bios.BIOSVersion = getString(bios.BIOSVersion_);
                    bios.BIOSReleaseDate = getString(bios.BIOSReleaseDate_);
                }
                // 2.4+
                if (version_ >= SMBIOS_2_4)
                {
                    bios.ExtensionByte1 = reader.ReadByte();
                    bios.ExtensionByte2 = reader.ReadByte();
                    bios.SystemBIOSMajorRelease = reader.ReadByte();
                    bios.SystemBIOSMinorRelease = reader.ReadByte();
                    bios.EmbeddedFirmwareMajorRelease = reader.ReadByte();
                    bios.EmbeddedFirmwareMinorRelease = reader.ReadByte();
                }
            }

            if (entry_.type == DMI_TYPE_MEMORY)
            {
                var mem = entry_ as TypeMemoryDevice;
                // 2.1+
                if (version_ >= SMBIOS_2_1)
                {
                    mem.PhysicalArrayHandle = reader.ReadUInt16();
                    mem.ErrorInformationHandle = reader.ReadUInt16();
                    mem.TotalWidth = reader.ReadUInt16();
                    mem.DataWidth = reader.ReadUInt16();
                    mem.Size = reader.ReadUInt16();
                    mem.FormFactor = reader.ReadByte();
                    mem.DeviceSet = reader.ReadByte();
                    mem.DeviceLocator_ = reader.ReadByte();
                    mem.BankLocator_ = reader.ReadByte();
                    mem.MemoryType = reader.ReadByte();
                    mem.TypeDetail = reader.ReadUInt16();

                    mem.DeviceLocator = getString(mem.DeviceLocator_);
                    mem.BankLocator = getString(mem.BankLocator_);
                }
                // 2.3+
                if (version_ >= SMBIOS_2_3)
                {
                    mem.Speed = reader.ReadUInt16();
                    mem.Manufacturer_ = reader.ReadByte();
                    mem.SerialNumber_ = reader.ReadByte();
                    mem.AssetTagNumber_ = reader.ReadByte();
                    mem.PartNumber_ = reader.ReadByte();

                    mem.Manufacturer = getString(mem.Manufacturer_);
                    mem.SerialNumber = getString(mem.SerialNumber_);
                    mem.AssetTagNumber = getString(mem.AssetTagNumber_);
                    mem.PartNumber = getString(mem.PartNumber_);
                }
                // 2.6+
                if (version_ >= SMBIOS_2_6)
                {
                    mem.Attributes = reader.ReadByte();
                }
                // 2.7+
                if (version_ >= SMBIOS_2_7)
                {
                    mem.ExtendedSize = reader.ReadUInt32();
                    mem.ConfiguredClockSpeed = reader.ReadUInt16();
                }
                // 2.8+
                if (version_ >= SMBIOS_2_8)
                {
                    mem.MinimumVoltage = reader.ReadUInt16();
                    mem.MinimumVoltage = reader.ReadUInt16();
                    mem.ConfiguredVoltage = reader.ReadUInt16();
                }
            }

            if (entry_.type == DMI_TYPE_PHYSMEM)
            {
                var phymem = entry_ as TypePhysicalMemory;
                // 2.1+
                if (version_ >= SMBIOS_2_1)
                {
                    phymem.Location = reader.ReadByte();
                    phymem.Use = reader.ReadByte();
                    phymem.ErrorCorrection = reader.ReadByte();
                    phymem.MaximumCapacity = reader.ReadUInt32();
                    phymem.ErrorInformationHandle = reader.ReadUInt16();
                    phymem.NumberDevices = reader.ReadUInt16();
                }
                // 2.7+
                if (version_ >= SMBIOS_2_7)
                {
                    phymem.ExtendedMaximumCapacity = reader.ReadUInt64();
                }
            }

            if (entry_.type == DMI_TYPE_BASEBOARD)
            {
                var baza = entry_ as TypeBaseboard;
                // 2.0+
                if (version_ >= SMBIOS_2_0)
                {
                    baza.Manufacturer_ = reader.ReadByte();
                    baza.Product_ = reader.ReadByte();
                    baza.Version_ = reader.ReadByte();
                    baza.SerialNumber_ = reader.ReadByte();
                    baza.AssetTag_ = reader.ReadByte();
                    baza.FeatureFlags = reader.ReadByte();
                    baza.LocationInChassis_ = reader.ReadByte();
                    baza.ChassisHandle = reader.ReadUInt16();
                    baza.BoardType = reader.ReadByte();
                    baza.NoOfContainedObjectHandles = reader.ReadByte();
                    //baza.ContainedObjectHandles = (uint16_t*)ptr_;
                    //ptr_ += entry_.data.baseboard.NoOfContainedObjectHandles * sizeof(uint16_t);
                    baza.Manufacturer = getString(baza.Manufacturer_);
                    baza.Product = getString(baza.Product_);
                    baza.Version = getString(baza.Version_);
                    baza.SerialNumber = getString(baza.SerialNumber_);
                    baza.AssetTag = getString(baza.AssetTag_);
                    baza.LocationInChassis = getString(baza.LocationInChassis_);
                }
            }

            if (entry_.type == DMI_TYPE_PROCESSOR)
            {
                var proc = entry_ as TypeProcessor;
                // version 2.0
                if (version_ >= SMBIOS_2_0)
                {
                    proc.SocketDesignation_ = reader.ReadByte();
                    proc.ProcessorType = reader.ReadByte();
                    proc.ProcessorFamily = reader.ReadByte();
                    proc.ProcessorManufacturer_ = reader.ReadByte();
                    for (int i = 0; i < 8; ++i)
                        proc.ProcessorID[i] = reader.ReadByte();
                    proc.ProcessorVersion_ = reader.ReadByte();
                    proc.Voltage = reader.ReadByte();
                    proc.ExternalClock = reader.ReadUInt16();
                    proc.MaxSpeed = reader.ReadUInt16();
                    proc.CurrentSpeed = reader.ReadUInt16();
                    proc.Status = reader.ReadByte();
                    proc.ProcessorUpgrade = reader.ReadByte();

                    proc.SocketDesignation = getString(proc.SocketDesignation_);
                    proc.ProcessorManufacturer = getString(proc.ProcessorManufacturer_);
                    proc.ProcessorVersion = getString(proc.ProcessorVersion_);
                }
                if (version_ >= SMBIOS_2_1)
                {
                    proc.L1CacheHandle = reader.ReadUInt16();
                    proc.L2CacheHandle = reader.ReadUInt16();
                    proc.L3CacheHandle = reader.ReadUInt16();
                }
                if (version_ >= SMBIOS_2_3)
                {
                    proc.SerialNumber_ = reader.ReadByte();
                    proc.AssetTagNumber_ = reader.ReadByte();
                    proc.PartNumber_ = reader.ReadByte();

                    proc.SerialNumber = getString(proc.SerialNumber_);
                    proc.AssetTagNumber = getString(proc.AssetTagNumber_);
                    proc.PartNumber = getString(proc.PartNumber_);
                }
                if (version_ >= SMBIOS_2_5)
                {
                    proc.CoreCount = reader.ReadByte();
                    proc.CoreEnabled = reader.ReadByte();
                    proc.ThreadCount = reader.ReadByte();
                    proc.ProcessorCharacteristics = reader.ReadUInt16();
                }
                if (version_ >= SMBIOS_2_6)
                {
                    proc.ProcessorFamily2 = reader.ReadUInt16();
                }
                if (version_ >= SMBIOS_3_0)
                {
                    proc.CoreCount2 = reader.ReadUInt16();
                    proc.CoreEnabled2 = reader.ReadUInt16();
                    proc.ThreadCount2 = reader.ReadUInt16();
                }
            }
        }

        private void InvalidData()
        {
            throw new ApplicationException();
        }
    }
}
