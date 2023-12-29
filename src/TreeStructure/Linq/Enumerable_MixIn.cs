using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Internals;

namespace TreeStructures.Linq {
    /// <summary>Extension methods for <see cref="IEnumerable{T}"/>.</summary>
    public static class EnumerableExtensions {
        /// <summary>Converts to a read-only object.</summary>
        public static IEnumerable<T> AsReadOnly<T>(this IEnumerable<T> enumerable) {
            return new EnumerableCollection<T>(enumerable);
        }
        
        internal class EnumerableCollection<T> : IEnumerable<T>,IReadOnlyList<T> {
            public EnumerableCollection(IEnumerable<T> collection) {
                _collection = collection;
            }
            public T this[int index] => _collection.ElementAt(index);

            public int Count => _collection.Count();
            IEnumerable<T> _collection;
            public IEnumerator<T> GetEnumerator() {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _collection.GetEnumerator();
            }
        }

        /// <summary>Combines disposable collections.</summary>
        public static IDisposable CombineDisposables<T>(this IEnumerable<T> enumerable) where T:IDisposable {
            return new LumpedDisopsables(enumerable.OfType<IDisposable>());
        }
        /// <summary>Generates an instance for traversing the sequence.</summary>
        public static SequenceScroller<T> ToSequenceScroller<T> (this IEnumerable<T> sequence) {
            return new SequenceScroller<T>(sequence);
        }
        //public static int FirstIndex<T>(this IEnumerable<T> ie, Predicate<T> match) {
        //    var t = ie.Select((tData, index) => new { tData, index }).FirstOrDefault(arg => match(arg.tData));
        //    if (t == null) return -1;
        //    else return t.index;
        //}

        //public static int? FirstIndexOrNull<T>(this IEnumerable<T> ie, Predicate<T> match) {
        //    return ie.Select((tData, index) => new { tData, index }).FirstOrDefault(arg => match(arg.tData))?.index;
        //}
    }
    
}
