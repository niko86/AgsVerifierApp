using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AgsVerifierLibrary.Models
{
    public class AgsRowModel : IEnumerable<object>
    {
        private readonly AgsGroupModel _group;
        private readonly int _rowIndex;
        internal AgsRowModel(AgsGroupModel group, int rowIndex)
        {
            _group = group;
            _rowIndex = rowIndex;
        }

        public IEnumerator<object> GetEnumerator()
        {
            foreach (AgsColumnModel column in _group.Columns)
            {
                yield return column.Data[_rowIndex];
            }
        }

        public object this[int index]
        {
            get
            {
                return _group.Columns[index][_rowIndex];
            }
            set
            {
                _group.Columns[index][_rowIndex] = (string) value;
            }
        }

        public object this[string columnName]
        {
            get
            {
                return _group.Columns.FirstOrDefault(c => c.Heading == columnName)[_rowIndex];
            }
            set
            {
                _group.Columns.FirstOrDefault(c => c.Heading == columnName)[_rowIndex] = (string) value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (object value in this)
            {
                sb.Append(value?.ToString() ?? "null").Append(' ');
            }
            return sb.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
