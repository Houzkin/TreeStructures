using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using TreeStructures.Events;
using TreeStructures.Internals;
using TreeStructures.Linq;
using TreeStructures.Results;
using TreeStructures.Tree;
using TreeStructures.Utilities;

namespace TreeStructures.Collections {

	/// <summary>
	/// Provides a collection that allows subscribing to property change notifications for each element in bulk.  
	/// The properties to be subscribed to can also be nested.  
	/// To subscribe to nested properties, the nested objects must also implement <see cref="INotifyPropertyChanged"/>.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the collection. To receive property change notifications, the elements must implement <see cref="INotifyPropertyChanged"/>.</typeparam>
	public class ReadOnlyObservableItemCollection<T> : IReadOnlyList<T>, IDisposable,INotifyCollectionChanged {

		ImitableCollection<T,ObservedPropertyTree<T>> _trees;
		List<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>,IDisposable>>> dim3 = new();
		IDisposable? listener;

		/// <summary>
		/// Returns the <see cref="IEnumerable{T}"/>that the <see cref="ReadOnlyObservableItemCollection{T}"/> wraps.
		/// </summary>
		protected virtual IEnumerable<T> Items { get; }

		/// <summary>Initializes a new instance.</summary>
		/// <param name="list">The source collection to be referenced. To enable synchronization, the collection must implement <see cref="INotifyCollectionChanged"/>.</param>
		public ReadOnlyObservableItemCollection(IEnumerable<T> list) {

			Items = list;
			_trees = ImitableCollection.Create(list,
				x => {
					var opt = new ObservedPropertyTree<T>(x);
					foreach (var d3 in dim3) {
						foreach (var d2 in d3.Keys) {
							var sbsc = opt.Subscribe(d2, handleItemPropertyChanged);
							d3[d2][opt] = sbsc;
						}
					}
					return opt;
				}, x => {
					foreach (var d3 in dim3) {
						foreach (var d2 in d3.Keys) {
							ResultWith<IDisposable>.Of(d3[d2].Remove,x).When(o => o.Dispose());
						}
					}
					x.Dispose();
				});
			if(Items is INotifyCollectionChanged notify) {
				listener = new EventListener<NotifyCollectionChangedEventHandler>(
					h => notify.CollectionChanged += h,
					h => notify.CollectionChanged -= h,
					(s, e) => this.CollectionChanged?.Invoke(this, e));
			}
		}

		void addExpressions(Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>,IDisposable>> area,IEnumerable<Expression<Func<T,object>>> exps) {
			ThrowExceptionIfDisposed();
			foreach(var key in exps) {
				if (area.ContainsKey(key)) continue;
//#if NETSTANDARD2_0
				var newDic = new Dictionary<ObservedPropertyTree<T>,IDisposable>();
				foreach (var trr in _trees) newDic.Add(trr, trr.Subscribe(key, this.handleItemPropertyChanged));
				area[key] = newDic;
//#else
//				area[key] = new Dictionary<ObservedPropertyTree<T>, IDisposable>(
//					_trees.Select(trr => KeyValuePair.Create(trr, trr.Subscribe(key, this.handleItemPropertyChanged))));
//#endif
			}
		}
		void removeExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area, IEnumerable<Expression<Func<T, object>>> exps) {
			foreach (var key in exps) {
				ResultWith<Dictionary<ObservedPropertyTree<T>, IDisposable>>.Of( area.Remove, key)
					.When( o => {
						foreach (var t in o.Values) t.Dispose();
					});
			}
		}
		void clearExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area) {
			foreach (var disp in area.Values.SelectMany(x=>x.Values)) { disp.Dispose(); }
		}
		void dispExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area) {
			if (dim3.Remove(area)) { clearExpressions(area); }
		}
		/// <summary>
		/// Acquires a collection for managing properties to subscribe to.  
		/// If the collection already contains the same <see cref="Expression{Func{T,object }}"/>, it will be excluded.  
		/// If duplicates are allowed, a new collection should be acquired.
		/// </summary>
		/// <param name="exps">The collection of property expressions to subscribe to.</param>
		/// <returns>A collection for adding and removing properties to subscribe to.</returns>
		protected ExpressionSubscriptionList AcquireNewSubscriptionList(IEnumerable<Expression<Func<T,object>>> exps){
			//var dic = new Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>, IDisposable>>();
			//var expc = new ExpressionSubscriptionList(dic,addExpressions, removeExpressions, clearExpressions, dispExpressions) {
			//	exps
			//};
			//return expc;
			var esp = this.AcquireNewSubscriptionList();
			esp.Add(exps);
			return esp;
		}
		/// <summary>
		/// Acquires a collection for managing properties to subscribe to.  
		/// If the collection already contains the same <see cref="Expression{Func{T, object}}"/>, it will be excluded.  
		/// If duplicates are allowed, a new collection should be acquired.
		/// </summary>
		/// <returns>A collection for adding and removing properties to subscribe to.</returns>
		protected ExpressionSubscriptionList AcquireNewSubscriptionList() { //} => new ExpressionSubscriptionList(addExpressions, removeExpressions, clearExpressions, dispExpressions);
			var dic = new Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>, IDisposable>>();
			dim3.Add(dic);
			return new ExpressionSubscriptionList(dic, addExpressions, removeExpressions, clearExpressions, dispExpressions);
		}

		void handleItemPropertyChanged(object? sender, ChainedPropertyChangedEventArgs<object> e) {
			if (sender is T typedSender) {
				HandleItemPropertyChanged(typedSender, e);
			} else {
				HandleItemPropertyChanged(default!, e);
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
		/// <see cref="Subscribe(Expression{Func{T, object}}, Action{T, ChainedPropertyChangedEventArgs{object}})"/> method  
		/// and raises notifications to subscribers monitoring property changes within the collection.
		/// </remarks>
		protected virtual void HandleItemPropertyChanged(T sender,ChainedPropertyChangedEventArgs<object> e){
			ChainedPropertyChanged?.Invoke(sender, e);
		}
		event Action<T, ChainedPropertyChangedEventArgs<object>>? ChainedPropertyChanged;
		/// <summary>
		/// Subscribes to notifications of changes to the specified property.
		/// </summary>
		/// <param name="expression">An expression that specifies the property to monitor for changes.</param>
		/// <param name="handle">An action that handles the event when a property change notification is received.</param>
		/// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe from the property change notifications.</returns>
		public IDisposable Subscribe(Expression<Func<T,object>> expression,Action<T,ChainedPropertyChangedEventArgs<object>> handle) {
			var col = this.AcquireNewSubscriptionList(new Expression<Func<T, object>>[] { expression });
			Action<T, ChainedPropertyChangedEventArgs<object>> action = (s, e) => {
				if (PropertyUtils.GetPropertyPath(expression).SequenceEqual(e.ChainedProperties)) handle(s, e);
			};
			ChainedPropertyChanged += action;
			var disp = new DisposableObject(() => { 
				col.Dispose(); 
				ChainedPropertyChanged -= action;
			});
			return disp;
		}

		/// <summary>
		/// CollectionChanged event (per <see cref="INotifyCollectionChanged" />).
		/// </summary>
		public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;
		//	{
		//	add { if(Items is INotifyCollectionChanged ncc) ncc.CollectionChanged += value; }
		//	remove { if(Items is INotifyCollectionChanged ncc)ncc.CollectionChanged -= value; }
		//}

		bool isDisposed = false;
		///<inheritdoc/>
		public void Dispose() {
			if (isDisposed) { return; }
			this.Dispose(true);
		}
		/// <summary>Adds resource disposal in derived classes.</summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
				foreach(var d3 in this.dim3) {
					foreach(var d2 in d3.Values) {
						foreach(var d1 in d2) {
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
            if(isDisposed) throw new ObjectDisposedException(GetType().FullName,"The instance has already been disposed and cannot be operated on.");
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
				if(Items is IReadOnlyList<T> lst) return lst[index];
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

		/// <summary>
		/// Represents a collection used to edit the list of properties to be subscribed to.  
		/// Duplicate <see cref="Expression"/> instances are excluded from the collection.
		/// </summary>
		public class ExpressionSubscriptionList : IEnumerable<Expression<Func<T,object>>>, IDisposable {
			Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> _area;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>,IEnumerable<Expression<Func<T, object>>>> _addAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>,IEnumerable<Expression<Func<T, object>>>> _removeAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _clearAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _dispAction;
			internal ExpressionSubscriptionList(
				Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>,IDisposable>> area,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> addAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> removeAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> clearAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>>  dispAction) {
				_area = area;
				_addAction = addAction;
				_removeAction = removeAction;
				_clearAction = clearAction;
				_dispAction = dispAction;
			}
			/// <summary>
			/// Adds properties (expressed as <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>) to the subscription list.  
			/// Duplicate <c>Expression&lt;Func&lt;T, object&gt;&gt;</c> instances are excluded from the collection.
			/// </summary>
			/// <param name="expressions">The collection of expressions representing the properties to be added to the subscription list.</param>
			public void Add(IEnumerable<Expression<Func<T, object>>> expressions) {
				_addAction(_area,expressions);
			}
			/// <summary>
			/// Unsubscribes from the specified properties (expressed as <c>Expression&lt;Func&lt;T, object&gt;&gt;</c>).
			/// </summary>
			/// <param name="expressions">The collection of expressions representing the properties to unsubscribe from.</param>

			public void Remove(IEnumerable<Expression<Func<T, object>>> expressions) { _removeAction(_area, expressions); }
			/// <summary>Unsubscribes from all properties in the collection.</summary>
			public void Clear() { _clearAction(_area); }
			/// <summary>Unsubscribes from all properties and disposes of the allocated collection.</summary>
			public void Dispose() { _dispAction(_area); }
			/// <inheritdoc/>
			public IEnumerator<Expression<Func<T, object>>> GetEnumerator() { return _area.Keys.GetEnumerator(); }
			IEnumerator IEnumerable.GetEnumerator() { return _area.Keys.GetEnumerator(); }
		}
	}
}
