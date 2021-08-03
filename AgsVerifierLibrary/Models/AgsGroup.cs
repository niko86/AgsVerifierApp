using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Models
{
    public class AgsGroup
    {
        public AgsGroup()
        {

        }

        public string Name { get; set; }
        public int GroupRow { get; set; }
        public int HeadingRow { get; set; }
        public int UnitRow { get; set; }
        public int TypeRow { get; set; }
        public int FirstDataRow { get; set; }
        public string ParentGroup { get; set; }
        public List<AgsColumn> Columns { get; set; }

        public int RowCount => Columns[0].Data.Count;

        private AgsRowCollection _rows;
        public AgsRowCollection Rows
        {
            get
            {
                if (_rows is null && RowCount > 0)
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


    }
}
