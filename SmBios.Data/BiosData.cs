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
        public List<TableBios> Bios { get; set; }

        [DisplayName("Устройства памяти")]   
        public List<TableMemoryDevice> Memory { get; set; }

        [DisplayName("Массивы памяти")]
        public List<TablePhysicalMemory> PhyMemory { get; set; } 

        [DisplayName("Материнские платы")]
        public List<TableBaseboard> BaseBoard { get; set; }

        [DisplayName("Процессоры")]
        public List<TableProcessor> Processor { get; set; }

        public override string ToString()
        {
            var opts = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var x = JsonConvert.SerializeObject(this, opts);
            return x;
        }
           
    }
}
