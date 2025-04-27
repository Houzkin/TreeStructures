using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Events;
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Tree;
using TreeStructures.Utilities;

namespace TreeStructures.Collections {

	///// <summary>Provides a collection that can be sorted and filtered, synchronized with the specified collection.</summary>
	///// <typeparam name="T"></typeparam>
	//public class ReadOnlyObservableFilterSortCollection<T> : ReadOnlyObservableTrackingCollection<T> {
	//	/// <summary>Initializes a new instance.</summary>
	//	/// <param name="list">The collection to synchronize with.</param>
	//	/// <param name="equality"></param>
	//	public ReadOnlyObservableFilterSortCollection(IEnumerable<T> list, IEqualityComparer<T>? equality = null) : base(list) {
	//		this.equality = equality ??= EqualityComparer<T>.Default;
	//		_list = list;

	//		var items = new ObservableCollection<T>(list);
	//		////items.CollectionChanged += (s, e) => this.CollectionChanged?.Invoke(this, e);
	//		//Items = items;
	//		if (Items is INotifyCollectionChanged notify) {
	//			_displayListner = new EventListener<NotifyCollectionChangedEventHandler>(
	//				h => notify.CollectionChanged += h,
	//				h => notify.CollectionChanged -= h,
	//				//collectionchanged);
	//				(s, e) => this.OnCollectionChanged(e));
	//				//(s, e) => CollectionChanged?.Invoke(this, e));
	//		}
	//		if(list is INotifyCollectionChanged srcs) {
	//			_srcListner = new EventListener<NotifyCollectionChangedEventHandler>(
	//				h => srcs.CollectionChanged += h,
	//				h => srcs.CollectionChanged -= h,
	//				collectionchanged);
	//		}
	//		filterExps = this.AcquireNewTrackingList();
	//		comparerExps = this.AcquireNewTrackingList();
	//		Align();
	//	}

	//	private IEnumerable<T> _list;
	//	ListAligner<T,ObservableCollection<T>>? _listAligner;
	//	IDisposable? _displayListner;
	//	IDisposable? _srcListner;

	//	/// <summary>
	//	/// Provides functionality to align and rearrange the collection represented by the current instance, <see cref="Items"/>.
	//	/// </summary>
	//	private ListAligner<T, ObservableCollection<T>> ListAligner { 
	//		get => _listAligner ??= new ListAligner<T, ObservableCollection<T>>((ObservableCollection<T>)Items,move: (collection, ord, to) => {collection.Move(ord, to); },comparer:this.equality);
	//	}
	//	readonly IEqualityComparer<T> equality;
	//	///// <summary>
	//	///// Returns the underlying <see cref="ObservableCollection{T}"/> wrapped by <see cref="ReadOnlyObservableTrackingCollection{T}"/> as an <see cref="IEnumerable{T}"/>.
	//	///// </summary>
	//	//protected override sealed IEnumerable<T> Items { get; }

	//	/// <inheritdoc/>
	//	public override sealed event NotifyCollectionChangedEventHandler? CollectionChanged;

	//	Func<T, bool>? _filter;
	//	TrackingPropertyList<T> filterExps;
	//	TrackingPropertyList<T> comparerExps;

	//	IComparer<T>? _comparer;
	//	IComparer<T> _incCmpr = new IncompetentComparer<T>();
	//	void collectionchanged(object? sender, NotifyCollectionChangedEventArgs e){
	//		Align();
	//	}
	//	/// <summary>
	//	/// Handles the property change event for an item in the collection.
	//	/// </summary>
	//	/// <param name="sender">The item whose property has changed.</param>
	//	/// <param name="e">The event arguments containing details about the change.</param>
	//	/// <remarks>
	//	/// When overriding this method, be sure to call the base class method.  
	//	/// </remarks>
	//	protected override void HandleItemPropertyChanged(T sender, ChainedPropertyChangedEventArgs<object> e) {
	//		base.HandleItemPropertyChanged(sender, e);
	//		var sbcprps = filterExps.Concat(comparerExps);
	//		if (sbcprps.Select(x => PropertyUtils.GetPropertyPath(x)).Any(x => e.ChainedProperties.SequenceEqual(x)))
	//			Align();
	//	}
	//	/// <summary>
	//	/// Adjusts the arrangement of the collection represented by the current instance, <see cref="Items"/>.
	//	/// </summary>
	//	protected virtual void Align(){
	//		ListAligner.AlignBy(_list.Where(_filter ?? new(x => true)).OrderBy(x => x, _comparer ?? _incCmpr));
	//	}
	//	/// <summary>
	//	/// Filters the collection by the specified property. (Nesting is allowed)
	//	/// </summary>
	//	/// <param name="filterProperty">The property to filter the collection by, returning only elements that return true.</param>
	//	public void FilterProperty(Expression<Func<T, bool>> filterProperty){
	//		var func = filterProperty.Compile();
	//		ParameterExpression param = Expression.Parameter(typeof(T), "x");
	//		UnaryExpression body = Expression.Convert(filterProperty.Body, typeof(object));
	//		this.FilterBy(func, Expression.Lambda<Func<T, object>>(body, param));
	//	}
	//	/// <summary>
	//	/// Filters the collection by the specified function.
	//	/// </summary>
	//	/// <param name="filterFunc">The function used to filter the collection, returning only elements that return true.</param>
	//	/// <param name="triggerProperty">Specifies the property to reevaluate when an element's property changes. (Nesting is allowed)</param>
	//	/// <param name="triggerProperties">Additional properties to specify for reevaluation when an element's property changes. (Nesting is allowed)</param>
	//	public void FilterBy(Func<T,bool> filterFunc,Expression<Func<T,object>> triggerProperty, params Expression<Func<T,object>>[] triggerProperties){
	//		filterExps.Clear();
	//		_filter = filterFunc;
	//		IEnumerable<Expression<Func<T, object>>> obs = triggerProperties.AddHead(triggerProperty);
	//		filterExps.Add(obs);
	//		Align();
	//	}
	//	/// <summary>Clear filter.</summary>
	//	public void ClearFilter(){
	//		filterExps.Clear();
	//		_filter = null;
	//		Align();
	//	}
	//	/// <summary>
	//	/// Sorts the collection by the specified property value.
	//	/// </summary>
	//	/// <typeparam name="TKey">The type of the property value to be used as the key.</typeparam>
	//	/// <param name="keyProperty">The property to be used as the key. (Nesting is allowed)</param>
	//	/// <param name="keyComparer">The comparer used for sorting.</param>
	//	public void SortProperty<TKey>(Expression<Func<T,TKey>> keyProperty,IComparer<TKey>? keyComparer = null){
	//		var func = keyProperty.Compile();
	//		ParameterExpression param = Expression.Parameter(typeof(T), "x");
	//		UnaryExpression body = Expression.Convert(keyProperty.Body, typeof(object));
	//		this.SortBy(func,keyComparer,Expression.Lambda<Func<T,object>>(body, param));
	//	}
	//	/// <summary>
	//	/// Sorts the collection using the specified key.
	//	/// </summary>
	//	/// <typeparam name="TKey">The type of the key.</typeparam>
	//	/// <param name="getCompareKey">The function to get the key.</param>
	//	/// <param name="keyComparer">The comparer used for sorting.</param>
	//	/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change. (Nesting is allowed)</param>
	//	public void SortBy<TKey>(Func<T,TKey> getCompareKey,IComparer<TKey>? keyComparer=null,params Expression<Func<T,object>>[] triggerProperties){
	//		comparerExps.Clear();
	//		_comparer = new CustomComparer<T, TKey>(getCompareKey, keyComparer);
	//		comparerExps.Add(triggerProperties);
	//		Align();
	//	}
	//	/// <summary>
	//	/// Sorts the collection using the specified key.
	//	/// </summary>
	//	/// <typeparam name="TKey">The type of the key.</typeparam>
	//	/// <param name="getCompareKey">the function to get the key.</param>
	//	/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change.(Nesting is allowed)</param>
	//	public void SortBy<TKey>(Func<T,TKey> getCompareKey,params Expression<Func<T,object>>[] triggerProperties)where TKey: IComparable<TKey>{
	//		this.SortBy(getCompareKey,  null, triggerProperties);
	//	}
	//	/// <summary>Unsorts the collection.</summary>
	//	public void ClearSorts(){
	//		comparerExps.Clear();
	//		_comparer = null;
	//	}
	//	/// <inheritdoc/>
	//	protected override void Dispose(bool disposing) {
	//		if (disposing) {
	//			//if (this._list is INotifyCollectionChanged notify) {
	//			//	notify.CollectionChanged -= collectionchanged;
	//			//}
	//			_displayListner?.Dispose();
	//			_srcListner?.Dispose();
	//		}
	//		base.Dispose(disposing);
	//	}
	//	ReadOnlyObservableCollection<T>? _readonly;
	//	/// <summary>
	//	/// Gets this collection as a read-only <see cref="ReadOnlyObservableCollection{T}"/>.
	//	/// </summary>
	//	/// <returns>
	//	/// A <see cref="ReadOnlyObservableCollection{T}"/> corresponding to this instance.
	//	/// The first call creates and caches the instance, and subsequent calls return the cached instance.
	//	/// </returns>
	//	/// <remarks>
	//	/// Wraps the internal <see cref="ObservableCollection{T}"/> in a <see cref="ReadOnlyObservableCollection{T}"/>.
	//	/// Any changes made to the original collection will be reflected in the read-only collection,
	//	/// but modifications to the read-only collection itself are not allowed.
	//	/// </remarks>
	//	public ReadOnlyObservableCollection<T> AsReadOnlyObservableCollection()
	//		=> _readonly ??= new ReadOnlyObservableCollection<T>((ObservableCollection<T>)Items);

	//	private class IncompetentComparer<TItem> : IComparer<TItem> {
	//		public int Compare(TItem? x, TItem? y) {
	//			return 0;
	//		}
	//	}


	//}


}
