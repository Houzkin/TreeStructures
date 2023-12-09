using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructure.Collections;
using TreeStructure.Internals;

namespace TreeStructure.Linq {
    public static class LinqExtensions {
        public static IEnumerable<T> AsReadOnlyEnumerable<T>(this IEnumerable<T> enumerable) {
            return new EnumerableCollection<T>(enumerable);
        }
        internal class EnumerableCollection<T> : IEnumerable<T> {
            public EnumerableCollection(IEnumerable<T> collection) {
                _collection = collection;
            }
            IEnumerable<T> _collection;
            public IEnumerator<T> GetEnumerator() {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _collection.GetEnumerator();
            }
        }
        public static IDisposable ToLumpDisposables<T>(this IEnumerable<T> enumerable) where T:IDisposable {
            return new LumpedDisopsables(enumerable.OfType<IDisposable>());
        }
        /// <summary>シーケンス巡回用のインスタンスを生成する。</summary>
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
