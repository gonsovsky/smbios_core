using System.Collections.Generic;
using Newtonsoft.Json;

namespace SmBios.Data
{
    /// <summary>
    /// Интерпретированные данные SM-BIOS
    /// </summary>
    public class BiosData
    {
        public List<TableBios> Bios = new List<TableBios>();
        public List<TableMemoryDevice> Memory = new List<TableMemoryDevice>();
        public List<TablePhysicalMemory> PhyMemory = new List<TablePhysicalMemory>();
        public List<TableBaseboard> BaseBoard = new List<TableBaseboard>();
        public List<TableProcessor> Processor = new List<TableProcessor>();

        public override string ToString()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
