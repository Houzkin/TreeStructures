using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures.Internals {
    internal class EnumerableCollection<T> : IEnumerable<T>, IReadOnlyList<T> {
        public EnumerableCollection(IEnumerable<T> collection) {
            _collection = collection;
        }
        IEnumerable<T> _collection;
        //public T this[int index] => _list is null ? _collection.ElementAt(index) : _list[index];
        public T this[int index] {
            get {
                if (_collection is IReadOnlyList<T> rolst) return rolst[index];
                else if (_collection is IList<T> lst) return lst[index];
                else return _collection.ElementAt(index);
            }
        }

        //public int Count => _list is null ? _collection.Count() : _list.Count;
        public int Count {
            get {
                if (_collection is IReadOnlyList<T> rolst) return rolst.Count;
                else if (_collection is IList<T> lst) return lst.Count;
                else return _collection.Count();
            }
        }
        public IEnumerator<T> GetEnumerator() {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _collection.GetEnumerator();
        }
    }
}
