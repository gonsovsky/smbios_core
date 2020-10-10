using System;
using System.IO;

namespace SmBios.Extractor
{
    public class SmBiosStreamUnix : SmBiosStream
    {
        public SmBiosStreamUnix() : base(SmBiosNativeUnix.GetDmi())
        {
            Version = SmBiosNativeUnix.Version;
        }
    }

    internal static class SmBiosNativeUnix
    {
        internal const string SmBiosFolder = "/sys/firmware/dmi/tables/";
        internal const string DmiFile = "DMI";
        internal const string EntryFile = "smbios_entry_point";
        internal static int Version;

        internal static byte[] GetDmi()
        {
            byte[] Data = File.ReadAllBytes(Path.Combine(SmBiosFolder, DmiFile));
            byte[] Head = File.ReadAllBytes(Path.Combine(SmBiosFolder, EntryFile));
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

            return Data;
        }

        internal static void InvalidData()
        {
            throw new ApplicationException();
        }
    }
}