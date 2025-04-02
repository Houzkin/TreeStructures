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
	/// <typeparam name="TSrc">The type of the elements in the source collection.</typeparam>
	/// <typeparam name="TSrcList">The type of the source collection, which must implement <see cref="IEnumerable{TSrc}"/> and <see cref="INotifyCollectionChanged"/>.</typeparam>
	/// <typeparam name="TLinked">The type of the elements in the wrapped collection that are linked to <typeparamref name="TSrc"/>.</typeparam>
	public class ReadOnlyObservableEnumerableWrapper<TSrc,TSrcList,TLinked> : ReadOnlyObservableCollection<TLinked> where TSrcList : IEnumerable<TSrc>, INotifyCollectionChanged {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyObservableEnumerableWrapper{TSrc, TSrcList, TLinked}"/> class.
		/// </summary>
		/// <param name="observableList">The source collection that implements <see cref="INotifyCollectionChanged"/>.</param>
		/// <param name="convert">A function that converts <typeparamref name="TSrc"/> elements to <typeparamref name="TLinked"/>.</param>
		/// <param name="equality">A function that determines whether a given <typeparamref name="TLinked"/> element corresponds to a given <typeparamref name="TSrc"/> element.</param>
		public ReadOnlyObservableEnumerableWrapper(TSrcList observableList, Func<TSrc,TLinked> convert,Func<TLinked,TSrc,bool> equality):base(new ObservableCollection<TLinked>()) {
			_list = observableList;
			_Aligner = new ListAligner<TLinked, TSrc, ObservableCollection<TLinked>>(_Items, convert, equality, move: (lst,ord, to)=>lst.Move(ord,to));
			_list.CollectionChanged += (s, e) => this.Align();
			Align();
		}
		
		ListAligner<TLinked, TSrc, ObservableCollection<TLinked>> _Aligner;
		TSrcList _list;
		private ObservableCollection<TLinked> _Items => (this.Items as ObservableCollection<TLinked>)!;
		void Align() {
			_Aligner.AlignBy(_list);
		}
	}
	/// <summary>
	/// A specialized wrapper that provides a read-only, observable collection interface for an <see cref="IEnumerable{T}"/> that implements <see cref="INotifyCollectionChanged"/>.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	/// <typeparam name="TList">The type of the underlying collection that implements <see cref="IEnumerable{T}"/> and <see cref="INotifyCollectionChanged"/>.</typeparam>
	public class ReadOnlyObservableEnumerableWrapper<T,TList> : ReadOnlyObservableEnumerableWrapper<T, TList, T> where TList : IEnumerable<T>,INotifyCollectionChanged {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyObservableEnumerableWrapper{T,TList}"/> class.
		/// </summary>
		/// <param name="list">The source collection to wrap.</param>
		/// <param name="equality">An optional equality comparer used to determine element equivalence. If null, the default equality comparer for <typeparamref name="T"/> is used.</param>
		public ReadOnlyObservableEnumerableWrapper(TList list, IEqualityComparer<T>? equality = null) : base(list, x => x, (x, y) => equality?.Equals(x, y) ?? EqualityComparer<T>.Default.Equals(x, y)){

		}
	}
	//public class ReadOnlyCollectionNotifier<T, TList> : ReadOnlyObservableCollection<T>,IDisposable  where TList : IEnumerable<T>, INotifyCollectionChanged {
	//	public ReadOnlyCollectionNotifier(TList list, IEqualityComparer<T>? equality = null) : base(new ObservableCollection<T>()) {
	//		_list = list;
	//		_Aligner = new ListAligner<T, ObservableCollection<T>>(_Items, move: (lst, ord, to) => lst.Move(ord, to), comparer: equality);
	//		if(list is INotifyCollectionChanged notify) {
	//			listener = new EventListener<NotifyCollectionChangedEventHandler>(
	//				h => notify.CollectionChanged += h,
	//				h => notify.CollectionChanged -= h,
	//				(s, e) => { Align(); });
	//		}
	//	}

	//	private ObservableCollection<T> _Items => (this.Items as ObservableCollection<T>)!;
	//	TList _list;
	//	ListAligner<T, ObservableCollection<T>> _Aligner;
	//	IDisposable? listener;
	//	void Align() {
	//		_Aligner.AlignBy(_list);
	//	}
	//	public void Dispose() {
	//		listener?.Dispose();
	//		listener = null;
	//	}
	//}

}
