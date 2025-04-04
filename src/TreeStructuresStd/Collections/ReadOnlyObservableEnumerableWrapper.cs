using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using TreeStructures.Events;

namespace TreeStructures.Collections {
	/// <summary>
	/// Wraps an observable collection that implements <see cref="INotifyCollectionChanged"/> 
	/// and provides a read-only view with elements linked to the source elements.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the source collection.</typeparam>
	/// <typeparam name="TList">The type of the source collection, which must implement <see cref="IEnumerable{TSrc}"/> and <see cref="INotifyCollectionChanged"/>.</typeparam>
	/// <typeparam name="U">The type of the elements in the wrapped collection that are linked to <typeparamref name="T"/>.</typeparam>
	public class ReadOnlyObservableEnumerableWrapper<TList, T, U> : ReadOnlyObservableCollection<U> where TList : IEnumerable<T>, INotifyCollectionChanged {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyObservableEnumerableWrapper{TSrc, TSrcList, TLinked}"/> class.
		/// </summary>
		/// <param name="observableList">The source collection that implements <see cref="INotifyCollectionChanged"/>.</param>
		/// <param name="convert">A function that converts <typeparamref name="T"/> elements to <typeparamref name="U"/>.</param>
		/// <param name="equality">A function that determines whether a given <typeparamref name="U"/> element corresponds to a given <typeparamref name="T"/> element.</param>
		public ReadOnlyObservableEnumerableWrapper(TList observableList, Func<T, U> convert, Func<U, T, bool> equality) : base(new ObservableCollection<U>()) {
			_list = observableList;
			_convert = convert;
			_equality = equality;
			//_Aligner = new ListAligner<T, U, ObservableCollection<U>>(_Items, convert, equality, move: (lst, ord, to) => lst.Move(ord, to));
			_list.CollectionChanged += (s, e) => this.Align();
			Align();
		}
		TList _list;
		Func<T, U> _convert;
		Func<U, T, bool> _equality;
		ListAligner<T, U, ObservableCollection<U>>? _Aligner;

		protected virtual ListAligner<T,U,ObservableCollection<U>> Aligner 
			=> _Aligner ??= new ListAligner<T, U, ObservableCollection<U>>(_Items,_convert,_equality,move:(lst,ord,to)=>lst.Move(ord,to));
		private ObservableCollection<U> _Items => (this.Items as ObservableCollection<U>)!;
		void Align() {
			Aligner.AlignBy(_list);
		}
	}
	/// <summary>
	/// A specialized wrapper that provides a read-only, observable collection interface for an <see cref="IEnumerable{T}"/> that implements <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	/// <typeparam name="TList">The type of the underlying collection that implements <see cref="IEnumerable{T}"/> and <see cref="INotifyCollectionChanged"/>.</typeparam>
	public class ReadOnlyObservableEnumerableWrapper<TList,T> : ReadOnlyObservableEnumerableWrapper<TList, T, T> where TList : IEnumerable<T>, INotifyCollectionChanged {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyObservableEnumerableWrapper{T,TList}"/> class.
		/// </summary>
		/// <param name="list">The source collection to wrap.</param>
		/// <param name="equality">An optional equality comparer used to determine element equivalence. If null, the default equality comparer for <typeparamref name="T"/> is used.</param>
		public ReadOnlyObservableEnumerableWrapper(TList list, IEqualityComparer<T>? equality = null) : base(list, x => x, (x, y) => equality?.Equals(x, y) ?? EqualityComparer<T>.Default.Equals(x, y)) {

		}
	}
}
