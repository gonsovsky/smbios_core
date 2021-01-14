using System;
using System.Collections.Generic;
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
        internal readonly TableFlags Flags;
        internal int Idx;

        /// <summary>Create reader for stream with SM-BIOS</summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurred while opening the bios.</exception>
        public SmBiosReader(MemoryStream input, int version, TableFlags flags= TableFlags.All) : base(input)
        {
            Data = input.GetBuffer();
            if (Version == 0)
                Version = Const.SMBIOS_3_0;
            Version = version;
            Flags = flags;
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
                        if (!Flags.HasFlag(TableFlags.Bios))
                            break;
                        var readerBios = new ReaderBios<TableBios>(this, header);
                        var tableBios = readerBios.Result;
                        if (result.Bios==null)
                            result.Bios = new List<TableBios>();
                        result.Bios.Add(tableBios);
                        break;

                    case Const.DMI_TYPE_BASEBOARD:
                        if (!Flags.HasFlag(TableFlags.BaseBoard))
                            break;
                        var readerBaseboard = new ReaderBaseboard<TableBaseboard>(this, header);
                        var tableBaseboard = readerBaseboard.Result;
                        if (result.BaseBoard == null)
                            result.BaseBoard = new List<TableBaseboard>();
                        result.BaseBoard.Add(tableBaseboard);
                        break;

                    case Const.DMI_TYPE_MEMORY:
                        if (!Flags.HasFlag(TableFlags.MemoryDevice))
                            break;
                        var readerMemory = new ReaderMemory<TableMemoryDevice>(this, header);
                        var tableMemory = readerMemory.Result;
                        if (result.Memory == null)
                            result.Memory = new List<TableMemoryDevice>();
                        result.Memory.Add(tableMemory);
                        break;

                    case Const.DMI_TYPE_PHYSMEM:
                        if (!Flags.HasFlag(TableFlags.PhysMemory))
                            break;
                        var readerPhysicalMemory = new ReaderPhysicalMemory<TablePhysicalMemory>(this, header);
                        var tablePhysicalMemory = readerPhysicalMemory.Result;
                        if (result.PhyMemory == null)
                            result.PhyMemory = new List<TablePhysicalMemory>();
                        result.PhyMemory.Add(tablePhysicalMemory);
                        break;

                    case Const.DMI_TYPE_PROCESSOR:
                        if (!Flags.HasFlag(TableFlags.Processor))
                            break;
                        var readerProcessor = new ReaderProcessor<TableProcessor>(this, header);
                        var tableProcessor = readerProcessor.Result;
                        if (result.Processor == null)
                            result.Processor = new List<TableProcessor>();
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