using System.Diagnostics.CodeAnalysis;

namespace SmBiosCore
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class SmBiosConst
    {
        internal const int SMBIOS_2_0 = 0x0200;
        internal const int SMBIOS_2_1 = 0x0201;
        internal const int SMBIOS_2_2 = 0x0202;
        internal const int SMBIOS_2_3 = 0x0203;
        internal const int SMBIOS_2_4 = 0x0204;
        internal const int SMBIOS_2_5 = 0x0205;
        internal const int SMBIOS_2_6 = 0x0206;
        internal const int SMBIOS_2_7 = 0x0207;
        internal const int SMBIOS_2_8 = 0x0208;
        internal const int SMBIOS_3_0 = 0x0300;

        internal const int DMI_TYPE_END = 127;
        internal const int DMI_TYPE_BIOS = 0;
        //internal const int DMI_TYPE_SYSINFO = 1;
        internal const int DMI_TYPE_BASEBOARD = 2;
        //internal const int DMI_TYPE_SYSENCLOSURE = 3;
        internal const int DMI_TYPE_PROCESSOR = 4;
        //internal const int DMI_TYPE_SYSSLOT = 9;
        internal const int DMI_TYPE_PHYSMEM = 16;
        internal const int DMI_TYPE_MEMORY = 17;
    }
}
