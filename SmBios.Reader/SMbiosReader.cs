using System;
using System.IO;
using System.Linq;
using System.Text;
using SmBios.Data;
using SmBios.Reader.Readers;

namespace SmBios.Reader
{
    public class SmBiosReader: BinaryReader
    {
        internal readonly int Version;
        internal readonly byte[] Data;
        internal int Idx;

        /// <summary>Create reader for stream with SM-BIOS</summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the bios.</exception>
        public SmBiosReader(MemoryStream input, int version) : base(input)
        {
            Data = input.GetBuffer();
            if (Version == 0)
                Version = Const.SMBIOS_3_0;
            Version = version;
        }

        /// <summary>Parse whole SM-Bios into SmBios.Data.BiosData structure</summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the bios.</exception>
        public virtual BiosData ReadBios()
        {
            var result = new BiosData();
            while (BaseStream.Position < BaseStream.Length)
            {
                var header = ReadHeader();
                switch (header.Type)
                {
                    case Const.DMI_TYPE_END:
                        return result;

                    case Const.DMI_TYPE_BIOS:
                        var readerBios = new ReaderBios<TableBios>(this, header);
                        var tableBios = readerBios.Result;
                        result.Bios.Add(tableBios);
                        break;

                    case Const.DMI_TYPE_BASEBOARD:
                        var readerBaseboard = new ReaderBaseboard<TableBaseboard>(this, header);
                        var tableBaseboard = readerBaseboard.Result;
                        result.BaseBoard.Add(tableBaseboard);
                        break;

                    case Const.DMI_TYPE_MEMORY:
                        var readerMemory = new ReaderMemory<TableMemoryDevice>(this, header);
                        var tableMemory = readerMemory.Result;
                        result.Memory.Add(tableMemory);
                        break;

                    case Const.DMI_TYPE_PHYSMEM:
                        var readerPhysicalMemory = new ReaderPhysicalMemory<TablePhysicalMemory>(this, header);
                        var tablePhysicalMemory = readerPhysicalMemory.Result;
                        result.PhyMemory.Add(tablePhysicalMemory);
                        break;

                    case Const.DMI_TYPE_PROCESSOR:
                        var readerProcessor = new ReaderProcessor<TableProcessor>(this, header);
                        var tableProcessor = readerProcessor.Result;
                        result.Processor.Add(tableProcessor);
                        break;
                }
                BaseStream.Seek(Idx, SeekOrigin.Begin);
            }
            return result;
        }

        internal TableHeader ReadHeader()
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
            var values = new string[] { };
            if (unformatted.Length > 0)
                values = Encoding.ASCII.GetString(unformatted).Split('\0');
            var result = new TableHeader()
            {
                Values = values,
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