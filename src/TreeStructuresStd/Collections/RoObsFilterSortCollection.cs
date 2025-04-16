using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TreeStructures.Collections {
	/// <summary>
	/// Provides a collection that can be sorted and filtered, synchronized with the specified collection.<br/>
	/// After use, call <see cref="ReadOnlyObservableProxyCollection{T,U}.Dispose()"/> to unsubscribe from the collection change notifications.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ReadOnlyObservableFilterSortCollection<T> : ReadOnlyObservableProxyCollection<T> {
		/// <summary>Initializes a new instance.</summary>
		/// <param name="source">The collection to synchronize with.</param>
		/// <param name="equality"></param>
		public ReadOnlyObservableFilterSortCollection(IEnumerable<T> source,IEqualityComparer<T>? equality = null) 
			: base(source,null,null,equality) {
			_equality = equality ?? Equality<T>.ValueOrReferenceComparer;// EqualityComparer<T>.Default;
			_observer = new ReadOnlyObservableTrackingCollection<T>(source);
			_observer.TrackingPropertyChanged += (s, e) => Align();
			//_observer.CollectionChanged += (s, e) => Align();// { if (e.Action != NotifyCollectionChangedAction.Move) Align(); };
			_filterExps = _observer.CreateTrackingList();
			_comparExps = _observer.CreateTrackingList();
		}
		ReadOnlyObservableTrackingCollection<T> _observer;

		IEqualityComparer<T> _equality;

		ListAligner<T, ObservableCollection<T>>? _alinger;
		private ListAligner<T,ObservableCollection<T>> Aligner 
			=> _alinger ??= new ListAligner<T, ObservableCollection<T>>((ObservableCollection<T>)Items, move: (itms, ord, to) => itms.Move(ord, to), comparer: this._equality);

		Func<T, bool>? _filter;
		TrackingPropertyList<T> _filterExps;
		TrackingPropertyList<T> _comparExps;
		IComparer<T>? _comparer;
		IComparer<T> _incCmpr = new CustomComparer<T, bool>(x => true);
		

		void Align() {
			Aligner.AlignBy(this.SourceItems.Where(_filter ?? new (x=>true)).OrderBy(x=>x,_comparer ?? _incCmpr));
		}
		/// <inheritdoc/>
		protected override void ApplyCollectionChange(NotifyCollectionChangedEventArgs e) {
			Align();
		}
		/// <summary>
		/// Filters the collection by the specified property. (Nesting is allowed)
		/// </summary>
		/// <param name="filterProperty">The property to filter the collection by, returning only elements that return true.</param>
		public void FilterProperty(Expression<Func<T, bool>> filterProperty) {
			var func = filterProperty.Compile();
			ParameterExpression param = Expression.Parameter(typeof(T), "x");
			UnaryExpression body = Expression.Convert(filterProperty.Body, typeof(object));
			this.FilterBy(func, Expression.Lambda<Func<T, object>>(body, param));
		}
		/// <summary>
		/// Filters the collection by the specified function.
		/// </summary>
		/// <param name="filterFunc">The function used to filter the collection, returning only elements that return true.</param>
		/// <param name="triggerProperty">Specifies the property to reevaluate when an element's property changes. (Nesting is allowed)</param>
		/// <param name="triggerProperties">Additional properties to specify for reevaluation when an element's property changes. (Nesting is allowed)</param>
		public void FilterBy(Func<T,bool> filterFunc,Expression<Func<T,object>> triggerProperty,params Expression<Func<T, object>>[] triggerProperties) {
			_filterExps.Clear();
			_filter = filterFunc;
			var props = triggerProperties.Prepend(triggerProperty);
			_filterExps.Register(props);
			Align();
		}
		/// <summary>Clear filter.</summary>
		public void ClearFilter() {
			_filterExps.Clear();
			_filter = null;
			Align();
		}
		/// <summary>
		/// Sorts the collection by the specified property value.
		/// </summary>
		/// <typeparam name="TKey">The type of the property value to be used as the key.</typeparam>
		/// <param name="keyProperty">The property to be used as the key. (Nesting is allowed)</param>
		/// <param name="keyComparer">The comparer used for sorting.</param>
		public void SortProperty<TKey>(Expression<Func<T, TKey>> keyProperty, IComparer<TKey>? keyComparer = null) {
			var func = keyProperty.Compile();
			ParameterExpression param = Expression.Parameter(typeof(T), "x");
			UnaryExpression body = Expression.Convert(keyProperty.Body, typeof(object));
			this.SortBy(func, keyComparer, Expression.Lambda<Func<T, object>>(body, param));
		}
		/// <summary>
		/// Sorts the collection using the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="getCompareKey">The function to get the key.</param>
		/// <param name="triggerProperty">The properties to trigger re-evaluation when the element's properties change. (Nesting is allowed)</param>
		/// <param name="triggerProperties">Additional properties to specify for reevaluation when an element's property changes. (Nesting is allowed)</param>
		public void SortBy<TKey>(Func<T, TKey> getCompareKey,Expression<Func<T,object>> triggerProperty, params Expression<Func<T, object>>[] triggerProperties) where TKey : IComparable<TKey> {
			var tps = triggerProperties.Prepend(triggerProperty).ToArray();
			this.SortBy(getCompareKey, keyComparer: null, tps);
		}

		/// <summary>
		/// Sorts the collection using the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="getComparerKey">The function to get the key.</param>
		/// <param name="keyComparer">The comparer used for sorting.</param>
		/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change. (Nesting is allowed)</param>
		public void SortBy<TKey>(Func<T,TKey> getComparerKey,IComparer<TKey>? keyComparer =null,params Expression<Func<T, object>>[] triggerProperties) {
			_comparExps.Clear();
			_comparer = new CustomComparer<T,TKey>(getComparerKey,keyComparer);
			_comparExps.Register(triggerProperties);
			Align();
		}
		/// <summary>Unsorts the collection.</summary>
		public void ClearSorts() {
			_comparExps.Clear();
			_comparer = null;
			Align();
		}
		/// <inheritdoc/>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_observer.Dispose();
			}
			base.Dispose(disposing);
		}

	}
}
