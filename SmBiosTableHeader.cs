namespace SmBiosCore
{
    internal class SmBiosTableHeader
    {
        internal SmBiosReader Reader;
        internal byte Length;
        internal short Handle;
        internal byte Type;
        internal int VersionBi;
        internal string[] StringValues = { };

        internal string GetString(int b)
        {
            if (b <= 0) return "";
            if (b > StringValues.Length)
                return "";
            return StringValues[b - 1];
        }
    }
}
