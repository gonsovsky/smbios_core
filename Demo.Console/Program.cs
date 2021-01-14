using SmBios.Data;
using SmBios.Extractor;
using SmBios.Reader;

namespace Demo.Console
{
    class Program
    {
        static void Main()
        {
            var want = TableFlags.All;
            //want = TableFlags.BaseBoard | TableFlags.Processor;
            using (var stream = SmBiosExtractor.OpenRead())
            {
                using (var reader = new SmBiosReader(stream, stream.Version, want))
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
