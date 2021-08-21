using AgsVerifierLibrary.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AgsVerifierLibrary.Models
{
    public class AgsContainer : IEnumerable<AgsGroup>
    {
        public string FilePath { get; set; }

        public AgsVersion Version { get; set; }

        private List<AgsGroup> _groups = new();
        
        public List<AgsGroup> Groups
        {
            get => _groups;
            set => _groups = value;
        }

        
        public AgsGroup this[int groupIndex]
        {
            get => _groups[groupIndex];
            set => _groups[groupIndex] = value;
        }
        
        public AgsGroup this[string groupName]
        {
            get => _groups.FirstOrDefault(g => g.Name == groupName);

            set => _groups[_groups.FindIndex(c => c.Name == groupName)] = value;
        }
        
        public int Count => _groups.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<AgsGroup> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _groups[i];
            }
        }
    }
}
