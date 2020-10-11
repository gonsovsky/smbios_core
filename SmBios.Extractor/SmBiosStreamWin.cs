#if NETCOREAPP2_0
using System.Runtime.InteropServices;
using System.Linq;
#else
#endif
using System;

namespace SmBios.Extractor
{
    public class SmBiosStreamWin : SmBiosStream
    {
        public SmBiosStreamWin() : base(SmBiosWinNative.GetDmi())
        {
            Version = SmBiosWinNative.Version;
        }
    }

    internal static class SmBiosWinNative
    {
        internal static int Version;

        internal static void InvalidData()
        {
            throw new ApplicationException();
        }

#if NETCOREAPP2_0
        [DllImport("kernel32.dll")]
        internal static extern uint GetSystemFirmwareTable(
            uint firmwareTableProviderSignature,
            uint firmwareTableId,
            [Out, MarshalAs(UnmanagedType.LPArray)]
            byte[] pFirmwareTableBuffer,
            uint bufferSize);

        internal static byte[] GetDmi()
        {
            byte[] byteSignature = { (byte)'B', (byte)'M', (byte)'S', (byte)'R' };
            var signature = BitConverter.ToUInt32(byteSignature, 0);
            uint size = GetSystemFirmwareTable(signature, 0, null, 0);
            byte[] data = new byte[size];
            if (size != GetSystemFirmwareTable(signature, 0, data, size))
                InvalidData();
            Version = data[1] << 8 | data[2];
            data = data.Skip(8).ToArray();
            return data;
        }
#elif NET40
        internal static byte[] GetDmi()
        {
            var scope = new System.Management.ManagementScope("\\\\" + "." + "\\root\\WMI");
            scope.Connect();
            var wmiquery = new System.Management.ObjectQuery("SELECT * FROM MSSmBios_RawSMBiosTables");
            var searcher = new System.Management.ManagementObjectSearcher(scope, wmiquery);
            var coll = searcher.Get();
            var M_ByMajorVersion = 0;
            var M_ByMinorVersion = 0;
            byte[] data = null;
            foreach (var O in coll)
            {
                var queryObj = (System.Management.ManagementObject) O;
                if (queryObj["SMBiosData"] != null) data = (byte[]) (queryObj["SMBiosData"]);
                if (queryObj["SmbiosMajorVersion"] != null)
                    M_ByMajorVersion = (byte) (queryObj["SmbiosMajorVersion"]);
                if (queryObj["SmbiosMinorVersion"] != null)
                    M_ByMinorVersion = (byte) (queryObj["SmbiosMinorVersion"]);
                //if (queryObj["Size"] != null) m_dwLen = (long)(queryObj["Size"]);
                //m_dwLen = m_pbBIOSData.Length;
            }

            Version = (ushort) (M_ByMajorVersion << 8 | M_ByMinorVersion);

            return data;
        }
#endif
    }
}