namespace SmBios.Data
{
    public class TablePhysicalMemory : Table
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

}
