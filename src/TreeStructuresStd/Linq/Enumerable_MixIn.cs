using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Collections;
using TreeStructures.Internals;
using TreeStructures.Utilities;
using TreeStructures.Events;

namespace TreeStructures.Linq {
    /// <summary>Extension methods for <see cref="IEnumerable{T}"/>.</summary>
    public static class EnumerableExtensions {
        /// <summary>Converts to a read-only object.</summary>
        public static IEnumerable<T> AsReadOnly<T>(this IEnumerable<T> enumerable) {
            return new EnumerableCollection<T>(enumerable);
        }
		//public static ReadOnlyObservableCollection<T> AsReadOnlyObservable<T,TList>(this TList self) where TList:IEnumerable<T>,INotifyCollectionChanged {
		//	return new ReadOnlyNotifierCollection<T,TList>(self);
		//}
        
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
		public static ListScroller<T> ToListScroller<T>(this IEnumerable<T> list) {
			return new ListScroller<T>(list);
		}
		/// <summary>
		/// Aligns the elements of the current <see cref="IList{T}"/> with the specified original sequence.
		/// </summary>
		/// <typeparam name="T">The type of elements in the lists.</typeparam>
		/// <param name="self">The current list to align.</param>
		/// <param name="sequence">The original sequence to align with.</param>
		/// <param name="equality">An optional equality comparer for element comparison.</param>
		public static void AlignBy<T>(this IList<T> self,IEnumerable<T> sequence, IEqualityComparer<T>? equality = null) {
			var editor = new ListAligner<T, IList<T>>(self, comparer: equality);
			editor.AlignBy(sequence);
		}

		/// <summary>
		/// Aligns the elements of the current <see cref="ObservableCollection{T}"/> with the specified original sequence.
		/// </summary>
		/// <typeparam name="T">The type of elements in the observable collection.</typeparam>
		/// <param name="self">The current observable collection to align.</param>
		/// <param name="sequence">The original sequence to align with.</param>
		/// <param name="equality">An optional equality comparer for element comparison.</param>
		public static void AlignBy<T>(this ObservableCollection<T> self, IEnumerable<T> sequence, IEqualityComparer<T>? equality = null) {
			var editer = new ListAligner<T, ObservableCollection<T>>(self, move: (list, ord, to) => { list.Move(ord, to); }, comparer: equality);
			editer.AlignBy(sequence);
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

		/// <summary>
		/// Generates a new instance of <see cref="ReadOnlySortFilterObservableCollection{T}"/> with specified sorting and filtering options from the current observable collection.
		/// </summary>
		/// <typeparam name="T">The type of elements in the collection.</typeparam>
		/// <param name="self">The current observable collection.</param>
		/// <param name="equality">The equality comparer to use for comparing elements, or null to use the default comparer.</param>
		/// <returns>A new instance of <see cref="ReadOnlySortFilterObservableCollection{T}"/> with the specified options.</returns>
		public static ReadOnlySortFilterObservableCollection<T> ToSortFilterObservable<T>(this ReadOnlyObservableCollection<T> self,IEqualityComparer<T>? equality = null){
			var sfo = new ReadOnlySortFilterObservableCollection<T>(self,equality);
			return sfo;
		}
		/// <summary>
		/// Generates a new instance of <see cref="ReadOnlySortFilterObservableCollection{T}"/> with specified sorting and filtering options from the current observable collection.
		/// </summary>
		/// <typeparam name="T">The type of elements in the collection.</typeparam>
		/// <param name="self">The current observable collection.</param>
		/// <param name="equality">The equality comparer to use for comparing elements, or null to use the default comparer.</param>
		/// <returns>A new instance of <see cref="ReadOnlySortFilterObservableCollection{T}"/> with the specified options.</returns>
		public static ReadOnlySortFilterObservableCollection<T> ToSortFilterObservable<T>(this ObservableCollection<T> self,IEqualityComparer<T>? equality=null){
			return new ReadOnlySortFilterObservableCollection<T>(self, equality);
		}
#if NETSTANDARD2_0
		internal static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count) {
			var q = new Queue<TSource>();
			foreach (var x in source) { 
				q.Enqueue(x);
				if (q.Count > count) {
					yield return q.Dequeue();
				}
			}
		}
		internal static bool Remove<TKey,T>(this Dictionary<TKey,T> dictionary,TKey key,out T result) {
			if(dictionary.ContainsKey(key)) {
				result = dictionary[key];
			} else { result = default(T); }
			return dictionary.Remove(key);
		}

		internal static bool TryPop<T>(this Stack<T> stack, out T result) {
			result = default(T);

			if (stack.Count > 0) {
				result = stack.Pop();
				return true;
			}

			return false;
		}

		internal static bool TryPeek<T>(this Stack<T> stack, out T result) {
			result = default(T);

			if (stack.Count > 0) {
				result = stack.Peek();
				return true;
			}

			return false;
		}
#endif


	}

}
