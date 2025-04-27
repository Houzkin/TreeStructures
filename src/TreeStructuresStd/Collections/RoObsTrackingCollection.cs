using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TreeStructures.Events;
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Results;
using TreeStructures.Tree;
using TreeStructures.Utilities;

namespace TreeStructures.Collections {//ReadOnlyItemTrackingCollection

	/// <summary>
	/// Provides a collection that allows subscribing to property change notifications for each element in bulk.  
	/// The properties to be subscribed to can also be nested.  
	/// To subscribe to nested properties, the nested objects must also implement <see cref="INotifyPropertyChanged"/>.  
	/// After use, call <see cref="Dispose()"/> to unsubscribe from the collection and property change notifications.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the collection. To receive property change notifications, the elements must implement <see cref="INotifyPropertyChanged"/>.</typeparam>
	public class ReadOnlyObservableTrackingCollection<T> : IReadOnlyList<T>, IDisposable, INotifyCollectionChanged {

		ReadOnlyObservableProxyCollection<T, ObservedPropertyTree<T>> _trees;
		List<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> dim3 = new();
		IDisposable? listener;

		/// <summary>
		/// Gets the <see cref="IEnumerable{T}"/>that the <see cref="ReadOnlyObservableTrackingCollection{T}"/> wraps.
		/// </summary>
		protected IEnumerable<T> Items { get; }

		/// <summary>Initializes a new instance.</summary>
		/// <param name="items">The source collection to be referenced. To enable synchronization, the collection must implement <see cref="INotifyCollectionChanged"/>.</param>
		public ReadOnlyObservableTrackingCollection(IEnumerable<T> items) {

			Items = items;
			_trees = new ReadOnlyObservableProxyCollection<T, ObservedPropertyTree<T>>(
				items,
				x => {
					var opt = new ObservedPropertyTree<T>(x);
					foreach (var d3 in dim3) {
						foreach (var d2 in d3.Keys) {
							var sbsc = opt.Subscribe(d2, handleItemPropertyChanged);
							d3[d2][opt] = sbsc;
						}
					}
					return opt;
				},
				(itm, tree) => Equality.ValueOrReferenceComparer.Equals(itm, tree.Root.Source),// EqualityComparer<T>.Default.Equals(x, y.RootSource),
				y => {
					foreach (var d3 in dim3) {
						foreach (var d2 in d3.Keys) {
							ResultWith<IDisposable>.Of(d3[d2].Remove, y).When(o => o.Dispose());
						}
					}
					y.Dispose();
				});
			if (Items is INotifyCollectionChanged notify) {
				listener = new EventListener<NotifyCollectionChangedEventHandler>(
					h => notify.CollectionChanged += h,
					h => notify.CollectionChanged -= h,
					(s, e) => this.OnCollectionChanged(e));
			}
		}

		void addExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area, IEnumerable<Expression<Func<T, object>>> exps) {
			ThrowExceptionIfDisposed();
			foreach (var key in exps) {
				if (area.ContainsKey(key)) continue;
				var newDic = new Dictionary<ObservedPropertyTree<T>, IDisposable>();
				foreach (var trr in _trees) newDic.Add(trr, trr.Subscribe(key, this.handleItemPropertyChanged));
				area[key] = newDic;
			}
		}
		void removeExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area, IEnumerable<Expression<Func<T, object>>> exps) {
			foreach (var key in exps) {
				ResultWith<Dictionary<ObservedPropertyTree<T>, IDisposable>>.Of(area.Remove, key)
					.When(o => {
						foreach (var t in o.Values) t.Dispose();
					});
			}
		}
		void clearExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area) {
			foreach (var disp in area.Values.SelectMany(x => x.Values)) { disp.Dispose(); }
		}
		void dispExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area) {
			if (dim3.Remove(area)) { clearExpressions(area); }
		}
		/// <summary>
		/// Acquires a collection for managing properties to subscribe to.  
		/// If the collection already contains the same <see cref="Expression{Func{T,object }}"/>, it will be excluded.  
		/// If duplicates are allowed, a new collection should be acquired.
		/// </summary>
		/// <param name="properties">The collection of property expressions to subscribe to.</param>
		/// <returns>A collection for adding and removing properties to subscribe to.</returns>
		public TrackingPropertyList<T> CreateTrackingList(IEnumerable<Expression<Func<T, object>>> properties) {
			var esp = this.CreateTrackingList();
			esp.Register(properties);
			return esp;
		}
		/// <summary>
		/// Acquires a collection for managing properties to subscribe to.  
		/// If the collection already contains the same <see cref="Expression{Func{T, object}}"/>, it will be excluded.  
		/// If duplicates are allowed, a new collection should be acquired.
		/// </summary>
		/// <returns>A collection for adding and removing properties to subscribe to.</returns>
		public TrackingPropertyList<T> CreateTrackingList() { //} => new ExpressionSubscriptionList(addExpressions, removeExpressions, clearExpressions, dispExpressions);
			var dic = new Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>(
				Equality<Expression<Func<T, object>>>.ComparerBySequence(x => PropertyUtils.GetPropertyPath(x)));
			dim3.Add(dic);
			return new TrackingPropertyList<T>(this, dic, addExpressions, removeExpressions, clearExpressions, dispExpressions);
		}

		void handleItemPropertyChanged(object? sender, ChainedPropertyChangedEventArgs<object> e) {
			if (sender is T typedSender) {
				OnTrackingPropertyChanged(typedSender, e);
			} else {
				OnTrackingPropertyChanged(default!, e);
			}
		}
		/// <summary>
		/// Handles the property change event for an item in the collection.
		/// </summary>
		/// <param name="sender">The item whose property has changed.</param>
		/// <param name="e">The event arguments containing details about the change.</param>
		/// <remarks>
		/// When overriding this method, be sure to call the base class method.  
		/// The base class method invokes the delegate registered via the  
		/// <see cref="TrackHandler(Expression{Func{T, object}}, Action{T, ChainedPropertyChangedEventArgs{object}})"/> method  
		/// and raises notifications to subscribers monitoring property changes within the collection.
		/// </remarks>
		protected virtual void OnTrackingPropertyChanged(T sender, ChainedPropertyChangedEventArgs<object> e) {
			TrackingPropertyChanged?.Invoke(sender, e);
		}
		/// <summary>Occurs when a tracked property of any item in the collection changes.</summary>
		public virtual event Action<T, ChainedPropertyChangedEventArgs<object>>? TrackingPropertyChanged;
		/// <summary>
		/// Subscribes to notifications of changes to the specified property.
		/// </summary>
		/// <param name="property">An expression that specifies the property to monitor for changes.</param>
		/// <param name="handle">An action that handles the event when a property change notification is received.</param>
		/// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the property change notifications.</returns>
		public IDisposable TrackHandler(Expression<Func<T, object>> property, Action<T, ChainedPropertyChangedEventArgs<object>> handle) {
			return this.TrackHandler(new[] { property }, handle);
		}
		/// <summary>
		/// Subscribes to notifications of changes to the specified properties.
		/// </summary>
		/// <param name="properties">Property expressions that specifies the properties to monitor for changes.</param>
		/// <param name="handle">An action that handles the event when the specified property change notification is received.</param>
		/// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the property change notifications.</returns>
		public IDisposable TrackHandler(IEnumerable<Expression<Func<T, object>>> properties, Action<T, ChainedPropertyChangedEventArgs<object>> handle) {
			var props = properties.ToArray();
			var trakings = this.CreateTrackingList(props);
			trakings.AttachHandler(handle);
			return new DisposableObject(() => trakings.Dispose());
		}
		/// <summary>Raise CollectionChanged event to any listeners.</summary>
		/// <param name="e"></param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
			this.CollectionChanged?.Invoke(this, e);
		}

		/// <summary>
		/// CollectionChanged event (per <see cref="INotifyCollectionChanged" />).
		/// </summary>
		public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

		bool isDisposed = false;
		///<inheritdoc/>
		public void Dispose() {
			if (isDisposed) { return; }
			this.Dispose(true);
		}
		/// <summary>Releases the resources used by the collection.</summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				// TODO: マネージド状態を破棄します (マネージド オブジェクト)
				foreach (var d3 in this.dim3) {
					foreach (var d2 in d3.Values) {
						foreach (var d1 in d2) {
							d1.Value.Dispose();
							d1.Key.Dispose();
						}
					}
				}
				_trees.Dispose();
				listener?.Dispose();
			}
			// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
			// TODO: 大きなフィールドを null に設定します
			isDisposed = true;
		}

		void ThrowExceptionIfDisposed() {
			if (isDisposed) throw new ObjectDisposedException(GetType().FullName, "The instance has already been disposed and cannot be operated on.");
		}
		/// <inheritdoc/>
		public int Count {
			get {
				if (Items is IReadOnlyCollection<T> lst) return lst.Count;
				else if (Items is IList<T> ilist) return ilist.Count;
				else return Items.Count();
			}
		}
		/// <inheritdoc/>
		public T this[int index] {
			get {
				if (Items is IReadOnlyList<T> lst) return lst[index];
				else if (Items is IList<T> ilist) return ilist[index];
				else return Items.ElementAt(index);
			}
		}
		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator() {
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable)Items).GetEnumerator();
		}
	}
	public class ExpressionList<T> : IEnumerable<Expression<Func<T, object>>> {
		List<Expression<Func<T, object>>> _list = new List<Expression<Func<T, object>>>();
		protected ExpressionList() { }
		public ExpressionList(Expression<Func<T, object>> property,params Expression<Func<T, object>>[] properties) {
			_list.Add(property);
			_list.AddRange(properties);
		}

		public virtual IEnumerator<Expression<Func<T, object>>> GetEnumerator() {
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	/// <summary>
	/// Represents a collection used to edit the list of properties to be subscribed to.  
	/// Duplicate <see cref="Expression"/> instances are excluded from the collection.
	/// </summary>
	public class TrackingPropertyList<T> : ExpressionList<T>, IDisposable {
		ReadOnlyObservableTrackingCollection<T> _self;
		Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> _area;
		Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> _addAction;
		Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> _removeAction;
		Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _clearAction;
		Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _dispAction;
		bool isDisposed = false;

		internal TrackingPropertyList(
			ReadOnlyObservableTrackingCollection<T> self,
			Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area,
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> addAction,
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> removeAction,
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> clearAction,
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> dispAction) {
			_self = self;
			_area = area;
			_addAction = addAction;
			_removeAction = removeAction;
			_clearAction = clearAction;
			_dispAction = dispAction;
		}

		/// <summary>
		/// Subscribes to notifications for the currently tracked properties.
		/// </summary>
		/// <param name="handle">The handler to invoke when a tracked property's value changes.</param>
		/// <returns>A disposable object used to unsubscribe from the notification.</returns>
		public IDisposable AttachHandler(Action<T, ChainedPropertyChangedEventArgs<object>> handle) {
			Action<T, ChainedPropertyChangedEventArgs<object>> action = (s, e) => {
				if (this.Select(x => PropertyUtils.GetPropertyPath(x)).Any(y => y.SequenceEqual(e.ChainedProperties))) 
					handle(s, e);
			};
			_self.TrackingPropertyChanged += action;
			var disp = new DisposableObject(() => {
				_self.TrackingPropertyChanged -= action;
			});
			return disp;
		}

		/// <summary>
		/// Adds properties (expressed as <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>) to the subscription list.  
		/// Duplicate <c>Expression&lt;Func&lt;T, object&gt;&gt;</c> instances are excluded from the collection.
		/// </summary>
		/// <param name="expressions">The collection of expressions representing the properties to be added to the subscription list.</param>
		public void Register(IEnumerable<Expression<Func<T, object>>> expressions) {
			this.ThrowExceptionIfDisposed();
			_addAction(_area, expressions);
		}

		/// <summary>
		/// Adds properties to the subscription list.  
		/// Duplicate <see cref="Expression"/> instances are excluded from the collection.
		/// </summary>
		/// <param name="expression">The first property expression to add.</param>
		/// <param name="expressions">Additional property expressions to add.</param>
		public void Register(Expression<Func<T, object>> expression, params Expression<Func<T, object>>[] expressions) {
			var exps = expressions .AddHead(expression);
			this.Register(exps);
		}

		/// <summary>
		/// Unsubscribes from the specified properties (expressed as <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>).
		/// </summary>
		/// <param name="expressions">The collection of expressions representing the properties to unsubscribe from.</param>
		public void Remove(IEnumerable<Expression<Func<T, object>>> expressions) {
			this.ThrowExceptionIfDisposed();
			_removeAction(_area, expressions);
		}

		/// <summary>
		/// Unsubscribes from the specified properties (expressed as <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>).
		/// </summary>
		/// <param name="expression">The first property expression to remove.</param>
		/// <param name="expressions">Additional property expressions to remove.</param>
		public void Remove(Expression<Func<T, object>> expression, params Expression<Func<T, object>>[] expressions) {
			var exps = expressions .AddHead (expression); 
			this.Remove(exps);
		}

		/// <summary>
		/// Unsubscribes from all properties in the collection.
		/// </summary>
		public void Clear() {
			this.ThrowExceptionIfDisposed();
			_clearAction(_area);
		}

		/// <summary>
		/// Unsubscribes from all properties and disposes of the allocated collection.
		/// </summary>
		public void Dispose() {
			if (isDisposed) return;
			_dispAction(_area);
			isDisposed = true;
		}

		void ThrowExceptionIfDisposed() {
			if (isDisposed)
				throw new ObjectDisposedException(GetType().FullName, "The instance has already been disposed and cannot be operated on.");
		}

		/// <inheritdoc/>
		public override IEnumerator<Expression<Func<T, object>>> GetEnumerator() => _area.Keys.GetEnumerator();
	}

}
