using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
		/// <summary>
		/// Aligns the elements of the current <see cref="IList{T}"/> with the specified original sequence.
		/// </summary>
		/// <typeparam name="T">The type of elements in the lists.</typeparam>
		/// <param name="self">The current list to align.</param>
		/// <param name="org">The original sequence to align with.</param>
		/// <param name="equality">An optional equality comparer for element comparison.</param>
		public static void AlignBy<T>(this IList<T> self, IEnumerable<T> org, IEqualityComparer<T>? equality = null) {
			var editer = new ListAligner<T, IList<T>>(self);
			editer.AlignBy(org, equality);
		}
		/// <summary>
		/// Aligns the elements of the current <see cref="ObservableCollection{T}"/> with the specified original sequence.
		/// </summary>
		/// <typeparam name="T">The type of elements in the observable collection.</typeparam>
		/// <param name="self">The current observable collection to align.</param>
		/// <param name="org">The original sequence to align with.</param>
		/// <param name="equality">An optional equality comparer for element comparison.</param>
		public static void AlignBy<T>(this ObservableCollection<T> self, IEnumerable<T> org, IEqualityComparer<T>? equality = null) {
			var editer = new ListAligner<T, ObservableCollection<T>>(self, move: (list, ord, to) => { list.Move(ord, to); });
			editer.AlignBy(org, equality);
		}

		/// <summary>
		/// Creates a new <see cref="CombinableObservableCollection{T}"/> from the specified enumerable, allowing combining collections and observing changes.
		/// </summary>
		/// <typeparam name="T">The type of elements in the collection.</typeparam>
		/// <param name="self">The enumerable to create a combinable collection from.</param>
		/// <param name="equality">An optional equality comparer for element comparison.</param>
		/// <returns>A new <see cref="CombinableObservableCollection{T}"/> instance containing elements from the specified enumerable.</returns>
		public static CombinableObservableCollection<T> ToCombinable<T>(this IEnumerable<T> self, IEqualityComparer<T>? equality = null){
			var coc = new CombinableObservableCollection<T>(equality);
			coc.AppendCollection(self);
			return coc;
		}

	}
    
}
