using AgsVerifierLibrary.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static AgsVerifierLibrary.Enums.EnumTools;

namespace AgsVerifierLibrary.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AgsColumn
    {
        public AgsColumn(AgsGroup group)
        {
            _group = group;
        }
        [JsonProperty]
        public string Heading { get; set; }
        [JsonProperty]
        public string Type { get; set; }
        [JsonProperty]
        public string Unit { get; set; }
        [JsonProperty]
        public List<dynamic> Data { get; set; } = new();

        private AgsGroup _group;
        public AgsGroup Group
        {
            get => _group;
            set => _group = value;
        }
        public string Status { get; set; }
        public string this[int rowIndex]
        {
            get => Data[rowIndex];
            set => Data[rowIndex] = value;
        }
        public int RowCount => Data.Count;
        public bool AllNull => Data.All(r => string.IsNullOrWhiteSpace(r));
        public IEnumerable<AgsRow> FilterRowsBy(string filter)
        {
            return _group.Rows.Where(r => r[Heading].Contains(filter));
        }
        public IEnumerable<AgsRow> FilterRowsBy(AgsDescriptor descriptor)
        {
            return _group.Rows.Where(r => r[Heading].ToString() == FastStr(descriptor));
        }
    }
}
