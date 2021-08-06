using AgsVerifierLibrary.Extensions;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Models.AgsEnum;

namespace AgsVerifierLibrary.Models
{
    public class AgsColumn
    {
        private readonly AgsGroup _group;

        public AgsColumn(AgsGroup group)
        {
            _group = group;
        }

        public string Heading { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public string MemberOf { get => _group.Name; }
        public List<dynamic> Data { get; } = new();

        public IEnumerable<AgsRow> FilterRowsBy(string filter)
        {
            return _group.Rows.Where(r => r[Heading].ToString().Contains(filter));
        }

        public IEnumerable<AgsRow> FilterRowsBy(int filter)
        {
            return _group.Rows.Where(r => (int) r[Heading] == filter);
        }

        public IEnumerable<AgsRow> FilterRowsBy(Descriptor descriptor)
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
