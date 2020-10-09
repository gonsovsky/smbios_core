using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SmBiosCore.Tables;

namespace SmBiosCore
{
    public class SmBios : IDisposable
    {
        public List<TableBios> Bios = new List<TableBios>();
        public List<TableMemoryDevice> Memory = new List<TableMemoryDevice>();
        public List<TablePhysicalMemory> PhyMemory = new List<TablePhysicalMemory>();
        public List<TableBaseboard> BaseBoard = new List<TableBaseboard>();
        public List<TableProcessor> Processor = new List<TableProcessor>();

        public void Dispose()
        {
        }

        public override string ToString()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
