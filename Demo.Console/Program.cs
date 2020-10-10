using SmBios.Extractor;
using SmBios.Reader;

namespace Demo.Console
{
    class Program
    {
        static void Main()
        {
            using (var stream = SmBiosExtractor.OpenRead())
            {
                using (var reader = new SmBiosReader(stream, stream.Version))
                {
                    var bios = reader.ReadBios();
                    var json = bios.ToString();
                    System.Console.WriteLine(json);
                    System.Console.ReadKey();
                }
            }
        }
    }
}
