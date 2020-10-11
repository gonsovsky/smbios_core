using System;
using System.IO;
using SmBios.Data;

namespace SmBios.Reader
{
    internal abstract class TableReader<T> where T: Table
    {
        private TableHeader header;

        internal TableReader(BinaryReader reader, TableHeader header)
        {
            Result = Activator.CreateInstance<T>();
        }

        internal T Result;

    }
}
