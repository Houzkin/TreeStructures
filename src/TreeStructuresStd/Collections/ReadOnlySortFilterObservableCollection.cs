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
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Tree;

namespace TreeStructures.Collections {

	/// <summary>Represents a collection that can be sorted and filtered, synchronized with the specified collection.</summary>
	/// <typeparam name="T"></typeparam>
	public class ReadOnlySortFilterObservableCollection<T> : ReadOnlyObservableItemCollection<T> {
		/// <summary>Initializes a new instance.</summary>
		/// <param name="list">The collection to synchronize with.</param>
		/// <param name="equality"></param>
		public ReadOnlySortFilterObservableCollection(IEnumerable<T> list, IEqualityComparer<T>? equality = null) : base(list) {
			Items = new ObservableCollection<T>(list);
			ListAligner = new ListAligner<T, ObservableCollection<T>>((ObservableCollection<T>)Items, move: (collection, ord, to) => { collection.Move(ord, to); });
			this.equality = equality ??= EqualityComparer<T>.Default;
			_list = list;
			if(list is INotifyCollectionChanged notify){
				notify.CollectionChanged += collectionchanged;
			}
			filterExps = this.AcquireNewSubscriptionList();
			comparerExps = this.AcquireNewSubscriptionList();
		}

		private IEnumerable<T> _list;
		protected ListAligner<T, ObservableCollection<T>> ListAligner { get; }
		readonly IEqualityComparer<T> equality;
		/// <inheritdoc/>
		protected override IEnumerable<T> Items { get; }
		Func<T, bool>? _filter;
		ExpressionSubscriptionList filterExps;
		ExpressionSubscriptionList comparerExps;

		IComparer<T>? _comparer;
		IComparer<T> _incCmpr = new IncompetentComparer<T>();
		void collectionchanged(object? sender, NotifyCollectionChangedEventArgs e){
			Align();
		}
		protected override void HandleItemPropertyChanged(T sender, ChainedPropertyChangedEventArgs<object> e) {
			base.HandleItemPropertyChanged(sender, e);
			Align();
		}

		void Align(){
			ListAligner.AlignBy(_list.Where(_filter ?? new(x => true)).OrderBy(x => x, _comparer ?? _incCmpr), this.equality);
		}
		/// <summary>
		/// Filters the collection by the specified property. (Nesting is allowed)
		/// </summary>
		/// <param name="filterProperty">The property to filter the collection by, returning only elements that return true.</param>
		public void FilterProperty(Expression<Func<T, bool>> filterProperty){
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
		public void FilterBy(Func<T,bool> filterFunc,Expression<Func<T,object>> triggerProperty, params Expression<Func<T,object>>[] triggerProperties){
			filterExps.Clear();
			_filter = filterFunc;
			IEnumerable<Expression<Func<T, object>>> obs = triggerProperties.Prepend(triggerProperty);
			filterExps.Add(obs);
			Align();
		}
		/// <summary>Clear filter.</summary>
		public void ClearFilter(){
			filterExps.Clear();
			_filter = null;
			Align();
		}
		/// <summary>
		/// Sorts the collection by the specified property value.
		/// </summary>
		/// <typeparam name="TKey">The type of the property value to be used as the key.</typeparam>
		/// <param name="keyProperty">The property to be used as the key. (Nesting is allowed)</param>
		/// <param name="keyComparer">The comparer used for sorting.</param>
		public void SortProperty<TKey>(Expression<Func<T,TKey>> keyProperty,IComparer<TKey>? keyComparer = null){
			var func = keyProperty.Compile();
			ParameterExpression param = Expression.Parameter(typeof(T), "x");
			UnaryExpression body = Expression.Convert(keyProperty.Body, typeof(object));
			this.SortBy(func,keyComparer,Expression.Lambda<Func<T,object>>(body, param));
		}
		/// <summary>
		/// Sorts the collection using the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="getCompareKey">The function to get the key.</param>
		/// <param name="keyComparer">The comparer used for sorting.</param>
		/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change. (Nesting is allowed)</param>
		public void SortBy<TKey>(Func<T,TKey> getCompareKey,IComparer<TKey>? keyComparer=null,params Expression<Func<T,object>>[] triggerProperties){
			comparerExps.Clear();
			_comparer = new CustomComparer<T, TKey>(getCompareKey, keyComparer);
			comparerExps.Add(triggerProperties);
			Align();
		}
		/// <summary>
		/// Sorts the collection using the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="getCompareKey">the function to get the key.</param>
		/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change.(Nesting is allowed)</param>
		public void SortBy<TKey>(Func<T,TKey> getCompareKey,params Expression<Func<T,object>>[] triggerProperties)where TKey: IComparable<TKey>{
			this.SortBy(getCompareKey,  null, triggerProperties);
		}
		/// <summary>Unsorts the collection.</summary>
		public void ClearSorts(){
			comparerExps.Clear();
			_comparer = null;
		}
		/// <inheritdoc/>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (this._list is INotifyCollectionChanged notify) {
					notify.CollectionChanged -= collectionchanged;
				}
			}
			base.Dispose(disposing);
		}
		ReadOnlyObservableCollection<T>? _readonly;
		/// <summary></summary>
		/// <returns></returns>
		public ReadOnlyObservableCollection<T> AsReadOnlyObservableCollection()
			=> _readonly ??= new ReadOnlyObservableCollection<T>((ObservableCollection<T>)Items);

		private class IncompetentComparer<TItem> : IComparer<TItem> {
			public int Compare(TItem? x, TItem? y) {
				return 0;
			}
		}


	}

	/*
		/// <summary>Initializes a new instance.</summary>
		/// <param name="list">The collection to synchronize with.</param>
		/// <param name="equality"></param>
		public ReadOnlySortFilterObservableCollection(IEnumerable<T> list, IEqualityComparer<T>? equality = null) : base(new ObservableCollection<T>()) {
			ListAligner = new ListAligner<T, ObservableCollection<T>>(_Items, move: (list, ord, to) => { list.Move(ord, to); });
			this.equality = equality ??= EqualityComparer<T>.Default;
			_list = list;
			if(list is INotifyCollectionChanged notify){
				notify.CollectionChanged += collectionchanged;
			}
			var fs = new List<IDisposable>();
			var cs = new List<IDisposable>();
			_trees = ImitableCollection.Create(list, x => {
				var opt = new ObservedPropertyTree<T>(x);
				foreach(var prop in _filterExpression){
					var s = opt.Subscribe(prop, Align);
					fs.Add(s);
					filterTrigger.Add(s);
				}	
				foreach(var prop in _comparerExpression){
					var s = opt.Subscribe(prop, Align);
					cs.Add(s);
					comparerTrigger.Add(s);
				}
				return opt;
			}, x => {
				filterTrigger.Remove(fs);
				comparerTrigger.Remove(cs);
				x.Dispose();
			});
			Align();
		}

		private ImitableCollection<T,ObservedPropertyTree<T>> _trees;
		private IEnumerable<T> _list;
		protected ListAligner<T, ObservableCollection<T>> ListAligner { get; }
		readonly IEqualityComparer<T> equality;
		private ObservableCollection<T> _Items => (this.Items as ObservableCollection<T>)!;
		Func<T, bool>? _filter;
		List<Expression<Func<T, object>>> _filterExpression = new();
		LumpedDisopsables filterTrigger = new();

		IComparer<T>? _comparer;
		IComparer<T> _incCmpr = new IncompetentComparer<T>();
		LumpedDisopsables comparerTrigger = new();
		List<Expression<Func<T, object>>> _comparerExpression = new();
		void collectionchanged(object? sender, NotifyCollectionChangedEventArgs e){
			Align();
		}

		void Align(){
			ListAligner.AlignBy(_list.Where(_filter ?? new(x => true)).OrderBy(x => x, _comparer ?? _incCmpr), this.equality);
		}
		/// <summary>
		/// Filters the collection by the specified property. (Nesting is allowed)
		/// </summary>
		/// <param name="filterProperty">The property to filter the collection by, returning only elements that return true.</param>
		public void FilterProperty(Expression<Func<T, bool>> filterProperty){
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
		public void FilterBy(Func<T,bool> filterFunc,Expression<Func<T,object>> triggerProperty, params Expression<Func<T,object>>[] triggerProperties){
			_filterExpression.Clear();
			filterTrigger.Dispose();

			_filter = filterFunc;
			IEnumerable<Expression<Func<T, object>>> obs = triggerProperties.Prepend(triggerProperty);
			_filterExpression.AddRange(obs);
			foreach(var item in _trees){
                foreach (var props in obs){
					filterTrigger.Add(item.Subscribe(props, Align));
				}
            }
			Align();
		}
		/// <summary>Clear filter.</summary>
		public void ClearFilter(){
			_filterExpression.Clear();
			filterTrigger.Dispose();
			_filter = null;
			Align();
		}
		/// <summary>
		/// Sorts the collection by the specified property value.
		/// </summary>
		/// <typeparam name="TKey">The type of the property value to be used as the key.</typeparam>
		/// <param name="keyProperty">The property to be used as the key. (Nesting is allowed)</param>
		/// <param name="keyComparer">The comparer used for sorting.</param>
		public void SortProperty<TKey>(Expression<Func<T,TKey>> keyProperty,IComparer<TKey>? keyComparer = null){
			var func = keyProperty.Compile();
			ParameterExpression param = Expression.Parameter(typeof(T), "x");
			UnaryExpression body = Expression.Convert(keyProperty.Body, typeof(object));
			this.SortBy(func,keyComparer,Expression.Lambda<Func<T,object>>(body, param));
		}
		/// <summary>
		/// Sorts the collection using the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="getCompareKey">The function to get the key.</param>
		/// <param name="keyComparer">The comparer used for sorting.</param>
		/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change. (Nesting is allowed)</param>
		public void SortBy<TKey>(Func<T,TKey> getCompareKey,IComparer<TKey>? keyComparer=null,params Expression<Func<T,object>>[] triggerProperties){
			_comparerExpression.Clear();
			comparerTrigger.Dispose();
			_comparer = new CustomComparer<T,TKey>(getCompareKey, keyComparer);
			_comparerExpression.AddRange(triggerProperties);
			foreach(var item in _trees){
				foreach(var props in triggerProperties){
					comparerTrigger.Add(item.Subscribe(props, Align));
				}
			}
			Align();
		}
		/// <summary>
		/// Sorts the collection using the specified key.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="getCompareKey">the function to get the key.</param>
		/// <param name="triggerProperties">The properties to trigger re-evaluation when the element's properties change.(Nesting is allowed)</param>
		public void SortBy<TKey>(Func<T,TKey> getCompareKey,params Expression<Func<T,object>>[] triggerProperties)where TKey: IComparable<TKey>{
			this.SortBy(getCompareKey,  null, triggerProperties);
		}
		/// <summary>Unsorts the collection.</summary>
		public void ClearSorts(){
			_comparerExpression.Clear();
			comparerTrigger.Dispose();
			_comparer = null;
		}
		bool isDisposed = false;
		/// <summary>Dispose instance.</summary>
		public void Dispose(){
			if (isDisposed) return;
			if(this._list is INotifyCollectionChanged notify){
				notify.CollectionChanged -= collectionchanged;
			}
			filterTrigger.Dispose();
			comparerTrigger.Dispose();
			_trees.Dispose();
			isDisposed = true;
		}
		ReadOnlyObservableCollection<T>? _readonly;
		/// <summary></summary>
		/// <returns></returns>
		public ReadOnlyObservableCollection<T> AsReadOnlyObservableCollection()
			=> _readonly ??= new ReadOnlyObservableCollection<T>(_Items);

		private class IncompetentComparer<TItem> : IComparer<TItem> {
			public int Compare(TItem? x, TItem? y) {
				return 0;
			}
		}


	}


	*/
}
