using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AgsVerifierLibrary.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AgsGroup
    {
        public AgsGroup()
        {
            Columns = new(); // Don't remove
            //AgsColumn indexColumn = AddColumn();
            //indexColumn.Heading = "Index";
        }

        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public List<AgsColumn> Columns { get; set; }
        [JsonProperty]
        public int GroupRow { get; set; }
        [JsonProperty]
        public int HeadingRow { get; set; }
        [JsonProperty]
        public int UnitRow { get; set; }
        [JsonProperty]
        public int TypeRow { get; set; }
        public AgsGroup ParentGroup { get; set; }
        public int RowCount => Columns[0].Data.Count;
        private AgsRowCollection _rows;
        public AgsRowCollection Rows
        {
            get
            {
                if (_rows is null )//&& RowCount > 0)
                    _rows = new AgsRowCollection(this);

                return _rows;
            }
        }
        public AgsColumn this[int columnIndex]
        {
            get => Columns[columnIndex];
            set => Columns[columnIndex] = value;
        }
        public AgsColumn this[string columnName]
        {
            get => Columns.FirstOrDefault(c => c.Heading == columnName);
            set => Columns[Columns.FindIndex(c => c.Heading == columnName)] = value;
        }
        public AgsColumn AddColumn()
        {
            var col = new AgsColumn(this);
            Columns.Add(col);
            return col;
        }
        public IEnumerable<AgsRow> Filter(string key, IEnumerable<dynamic> filters)
        {
            return Rows.Where(r => filters.Contains((string)r[key]));
        }
    }
}
