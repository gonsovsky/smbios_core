using System;
using Newtonsoft.Json;

namespace SmBiosCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new SmBiosCore();
            var output = JsonConvert.SerializeObject(x, Formatting.Indented);
            Console.WriteLine(output);
            Console.ReadKey();
        }
    }
}
