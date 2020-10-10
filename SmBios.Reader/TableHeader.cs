namespace SmBios.Reader
{
    internal class TableHeader
    {
        internal SmBiosReader Reader;
        internal byte Length;
        internal short Handle;
        internal byte Type;
        internal int VersionBi;
        internal string[] Values = { };

        internal string GetString(int b)
        {
            if (b <= 0) return "";
            if (b > Values.Length)
                return "";
            return Values[b - 1];
        }
    }
}
