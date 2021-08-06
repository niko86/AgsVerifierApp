﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Models
{
    public class AgsRow : IEnumerable<object>
    {
        private readonly AgsGroup _group;
        private readonly int _rowIndex;
        internal AgsRow(AgsGroup group, int rowIndex)
        {
            _group = group;
            _rowIndex = rowIndex;
        }

        public IEnumerator<dynamic> GetEnumerator()
        {
            foreach (var column in _group.Columns)
            {
                yield return column.Data[_rowIndex];
            }
        }

        public object this[int index]
        {
            get => _group.Columns[index][_rowIndex];
            set => _group.Columns[index][_rowIndex] = value;
        }

        public object this[string columnName]
        {
            get => _group.Columns.FirstOrDefault(c => c.Heading == columnName)[_rowIndex];
            set => _group.Columns.FirstOrDefault(c => c.Heading == columnName)[_rowIndex] = value;
        }

        public override string ToString()
        {
            return string.Join('|', this);
        }

        public int Index => (int)this[0];

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
