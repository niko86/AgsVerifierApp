using System;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Models
{
    public class AgsColumn
    {
        public string Heading { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public string PartOfGroup { get; set; }
        public List<string> Data { get; } = new();

        public string this[int rowIndex]
        {
            get => Data[rowIndex];
            set => Data[rowIndex] = value;
        }

        public int RowCount => Data.Count;
        public bool AllNull => Data.All(r => string.IsNullOrWhiteSpace(r));
    }
}
