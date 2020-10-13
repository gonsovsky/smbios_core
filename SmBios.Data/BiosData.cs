using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace SmBios.Data
{
    /// <summary>
    /// Интерпретированные данные SM-BIOS
    /// </summary>
    public class BiosData
    {
        [DisplayName("BIOS")]
        public List<TableBios> Bios { get; set; } = new List<TableBios>();

        [DisplayName("Устройства памяти")]   
        public List<TableMemoryDevice> Memory { get; set; } = new List<TableMemoryDevice>();

        [DisplayName("Массивы памяти")]
        public List<TablePhysicalMemory> PhyMemory { get; set; } = new List<TablePhysicalMemory>();

        [DisplayName("Материнские платы")]
        public List<TableBaseboard> BaseBoard { get; set; } = new List<TableBaseboard>();

        [DisplayName("Процессоры")]
        public List<TableProcessor> Processor { get; set; } = new List<TableProcessor>();

        public override string ToString()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
