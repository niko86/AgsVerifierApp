using AgsVerifierLibrary.Enums;
using AgsVerifierLibrary.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Models
{
    public class AgsColumn
    {
        private readonly AgsGroup _group;

        public AgsGroup Group => _group;

        public AgsColumn(AgsGroup group)
        {
            _group = group;
        }

        public string Heading { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public List<dynamic> Data { get; } = new();

        public IEnumerable<AgsRow> FilterRowsBy(string filter)
        {
            return _group.Rows.Where(r => r[Heading].ToString().Contains(filter));
        }

        public IEnumerable<AgsRow> FilterRowsBy(int filter)
        {
            return _group.Rows.Where(r => (int)r[Heading] == filter);
        }

        public IEnumerable<AgsRow> FilterRowsBy(AgsDescriptor descriptor)
        {
            return _group.Rows.Where(r => r[Heading].ToString() == descriptor.Name());
        }

        public object this[int rowIndex]
        {
            get => Data[rowIndex];
            set => Data[rowIndex] = value;
        }

        public int RowCount => Data.Count;
        public bool AllNull => Data.All(r => string.IsNullOrWhiteSpace(r));
    }
}
