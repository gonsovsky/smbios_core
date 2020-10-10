using System.IO;

namespace SmBios.Extractor
{
    public abstract class SmBiosStream: MemoryStream
    {
        protected SmBiosStream(byte[] buffer)
            : base(buffer, 0, buffer.Length, false, true )
        {
        }

        public int Version;
    }
}
