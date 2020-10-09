using System.IO;

namespace SmBiosCore.Tables
{

    public class TablePhysicalMemory : SmBiosTable
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

        internal TablePhysicalMemory(BinaryReader reader, SmBiosTableHeader header)
        {
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_1)
            {
                Location = reader.ReadByte();
                Use = reader.ReadByte();
                ErrorCorrection = reader.ReadByte();
                MaximumCapacity = reader.ReadUInt32();
                ErrorInformationHandle = reader.ReadUInt16();
                NumberDevices = reader.ReadUInt16();
            }
            // 2.7+
            if (header.VersionBi >= SmBiosConst.SMBIOS_2_7)
            {
                ExtendedMaximumCapacity = reader.ReadUInt64();
            }
        }
    }

}
