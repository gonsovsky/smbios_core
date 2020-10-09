using System;
using System.IO;
using System.Linq;
using System.Text;
using SmBiosCore.Tables;

namespace SmBiosCore
{
    public class SmBiosReader: BinaryReader
    {
        internal readonly int Version;
        internal readonly byte[] Data;
        internal int Idx;

        public SmBiosReader(SmBiosStream input) : base(input)
        {
            Data = input.GetBuffer();
            Version = input.Version;
        }

        public virtual SmBios ReadBios()
        {
            var result = new SmBios();
            while (BaseStream.Position < BaseStream.Length)
            {
                var header = ReadHeader();
                switch (header.Type)
                {
                    case SmBiosConst.DMI_TYPE_END:
                        return result;

                    case SmBiosConst.DMI_TYPE_BIOS:
                        var tableBios = new TableBios(this, header);
                        result.Bios.Add(tableBios);
                        break;

                    case SmBiosConst.DMI_TYPE_BASEBOARD:
                        var tableBaseboard = new TableBaseboard(this, header);
                        result.BaseBoard.Add(tableBaseboard);
                        break;

                    case SmBiosConst.DMI_TYPE_MEMORY:
                        var tableMemory = new TableMemoryDevice(this, header);
                        result.Memory.Add(tableMemory);
                        break;

                    case SmBiosConst.DMI_TYPE_PHYSMEM:
                        var tablePhysicalMemory = new TablePhysicalMemory(this, header);
                        result.PhyMemory.Add(tablePhysicalMemory);
                        break;

                    case SmBiosConst.DMI_TYPE_PROCESSOR:
                        var tableProcessor = new TableProcessor(this, header);
                        result.Processor.Add(tableProcessor);
                        break;
                }
                BaseStream.Seek(Idx, SeekOrigin.Begin);
            }
            return result;
        }

        internal SmBiosTableHeader ReadHeader()
        {
            var type = Data[Idx];
            var len = Data[Idx + 1];
            var handle = BitConverter.ToInt16(Data, Idx + 2);
            BaseStream.Seek(Idx + 4, SeekOrigin.Begin);
            byte[] unformatted;
            for (var j = Idx + len; ; j++)
            {
                if ((Data[j] == 0) && (Data[j + 1] == 0))
                {
                    unformatted = Data.Skip(Idx + len).Take(j - Idx - len).ToArray();
                    Idx = j + 2;
                    break;
                }
            }
            var vals = new string[] { };
            if (unformatted.Length > 0)
                vals = Encoding.ASCII.GetString(unformatted).Split('\0');
            var result = new SmBiosTableHeader()
            {
                StringValues = vals,
                Length = len,
                Type = type,
                Reader = this,
                Handle = handle,
                VersionBi = Version
            };
            return result;
        }
    }
}