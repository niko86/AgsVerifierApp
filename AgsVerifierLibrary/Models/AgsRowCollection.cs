using System;
using System.Collections;
using System.Collections.Generic;

namespace AgsVerifierLibrary.Models
{
    public class AgsRowCollection : IEnumerable<AgsRow>
    {
        private readonly AgsGroup _group;

        internal AgsRowCollection(AgsGroup group)
        {
            _group = group ?? throw new ArgumentNullException(nameof(_group));
        }

        public AgsRow this[int index]
        {
            get => new AgsRow(_group, index);
        }

        public IEnumerator<AgsRow> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return new AgsRow(_group, i);
            }
        }

        public int Count => _group.RowCount;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
