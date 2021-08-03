using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AgsVerifierLibrary.Models
{
    public class AgsRow : IEnumerable<string>
    {
        private readonly AgsGroup _group;
        private readonly int _rowIndex;
        internal AgsRow(AgsGroup group, int rowIndex)
        {
            _group = group;
            _rowIndex = rowIndex;
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach (var column in _group.Columns)
            {
                yield return column.Data[_rowIndex];
            }
        }

        public string this[int index]
        {
            get => _group.Columns[index][_rowIndex];
            set => _group.Columns[index][_rowIndex] = value;
        }

        public string this[string columnName]
        {
            get => _group.Columns.FirstOrDefault(c => c.Heading == columnName)[_rowIndex];
            set => _group.Columns.FirstOrDefault(c => c.Heading == columnName)[_rowIndex] = value;
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
