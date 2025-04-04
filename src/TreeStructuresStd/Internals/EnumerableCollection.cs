using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures.Internals {
    internal class EnumerableCollection<T> : IEnumerable<T>,IReadOnlyList<T> {
        public EnumerableCollection(IEnumerable<T> collection) {
            _collection = collection;
            if(collection is IReadOnlyList<T> list) { _list = list; }
        }
        IEnumerable<T> _collection;
        IReadOnlyList<T>? _list;
        public T this[int index] => _list is null ? _collection.ElementAt(index) : _list[index];

        public int Count => _list is null ? _collection.Count() : _list.Count;
        public IEnumerator<T> GetEnumerator() {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _collection.GetEnumerator();
        }
    }
}
