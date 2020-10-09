using System;

namespace SmBiosCore
{
    class Program
    {
        static void Main()
        {
            using (var stream = SmBiosExtractor.OpenRead())
            {
                using (var reader = new SmBiosReader(stream))
                {
                    using (var bios = reader.ReadBios())
                    {
                        Console.WriteLine(bios.ToString());
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
