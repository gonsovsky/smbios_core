using System;
using System.IO;
using SmBios.Data;

namespace SmBios.Reader
{
    internal abstract class TableReader<T> where T: Table
    {
        internal TableReader(BinaryReader reader, TableHeader header)
        {
            Result = Activator.CreateInstance<T>();
        }

        internal T Result;

    }
}
