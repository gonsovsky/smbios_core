#if NETCOREAPP
using System.Runtime.InteropServices;
#else

#endif

namespace SmBiosCore
{
    public static class SmBiosExtractor
    {
        /// <summary>Opens an existing SM-BIOS for reading.</summary>
        /// <returns>A read-only <see cref="T:System.IO.MemoryStream" /></returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the bios.</exception>
        public static SmBiosStream OpenRead()
        {
#if NETCOREAPP
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new SmBiosStreamWin();
            return new SmBiosStreamUnix();
#else
            return new SmBiosStreamWin();
#endif
        }
    }
}
