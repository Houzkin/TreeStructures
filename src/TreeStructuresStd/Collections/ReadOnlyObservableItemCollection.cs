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
using TreeStructures.Internals;
using TreeStructures.Tree;
using TreeStructures.Utility;

namespace TreeStructures.Collections {
	/// <summary>
	/// 各要素のプロパティ変更通知を、一括で購読できるコレクションを提供する。
	/// </summary>
	/// <typeparam name="T">各要素の型。プロパティ変更通知の購読を受け取るには<see cref="INotifyPropertyChanged"/>を実装している必要があります。</typeparam>
	public class ReadOnlyObservableItemCollection<T> : IEnumerable<T>,/*ReadOnlyObservableCollection<T>,*/ IDisposable,INotifyCollectionChanged {

		ImitableCollection<T,ObservedPropertyTree<T>> _trees;
		List<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>,IDisposable>>> dim3 = new();

		/// <summary>
		/// Returns the <see cref="IEnumerable{T}"/>that the <see cref="ReadOnlyObservableItemCollection{T}"/> wraps.
		/// </summary>
		protected virtual IEnumerable<T> Items => _trees.Source;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="list">参照元となるコレクション。連動させるには<see cref="INotifyCollectionChanged"/>を実装している必要があります。</param>
		public ReadOnlyObservableItemCollection(IEnumerable<T> list) /*: base(new ObservableCollection<T>(list))*/ {

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
		}
		void addExpressions(Dictionary<Expression<Func<T,object>>,Dictionary<ObservedPropertyTree<T>,IDisposable>> area,IEnumerable<Expression<Func<T,object>>> exps) {
			ThrowExceptionIfDisposed();
			foreach(var key in exps) {
				if (area.ContainsKey(key)) continue;
				area[key] = new Dictionary<ObservedPropertyTree<T>, IDisposable>(
					_trees.Select(trr => KeyValuePair.Create(trr, trr.Subscribe(key, this.handleItemPropertyChanged))));
			}
		}
		void removeExpressions(Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> area,IEnumerable<Expression<Func<T,object>>> exps) {
			foreach (var key in exps) {
				ResultWith<Dictionary<ObservedPropertyTree<T>, IDisposable>>.Of(area.Remove, key).When(
					o => {
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
		protected ExpressionSubscriptionList AcquireNewSubscriptionList(IEnumerable<Expression<Func<T,object>>> exps){
			var expc = new ExpressionSubscriptionList(addExpressions, removeExpressions, clearExpressions, dispExpressions) {
				exps
			};
			return expc;
		}
		/// <summary>
		/// 購読するプロパティを編集するためのコレクションを確保します。同一の<c>Expression&lt;Func&lt;T, object&gt;&gt;</c>が含まれる場合は除外されます。重複を許容する場合は、新たにコレクションを確保してください。
		/// </summary>
		/// <returns>購読するプロパティを追加・削除するためのコレクション。</returns>
		protected ExpressionSubscriptionList AcquireNewSubscriptionList() => new ExpressionSubscriptionList(addExpressions, removeExpressions, clearExpressions, dispExpressions);

		void handleItemPropertyChanged(object? sender, ChainedPropertyChangedEventArgs<object> e) {
			if (sender is T typedSender) {
				HandleItemPropertyChanged(typedSender, e);
			} else {
				HandleItemPropertyChanged(default!, e);
			}
		}
		/// <summary>
		/// 各要素からのプロパティ変更通知を受け取ったときに処理される。
		/// </summary>
		/// <param name="sender">通知を発行した要素</param>
		/// <param name="e">変更があったプロパティを示す</param>
		protected virtual void HandleItemPropertyChanged(T sender,ChainedPropertyChangedEventArgs<object> e){
			ChainedPropertyChanged?.Invoke(sender, e);
		}
		event Action<T, ChainedPropertyChangedEventArgs<object>>? ChainedPropertyChanged;

		/// <summary>
		/// 指定したプロパティの変更通知を購読する。
		/// </summary>
		/// <param name="expression">プロパティを指定</param>
		/// <param name="handle">通知を受け取った時の処理</param>
		/// <returns>イベントリスナー</returns>
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
		public event NotifyCollectionChangedEventHandler? CollectionChanged {
			add { if(Items is INotifyCollectionChanged ncc) ncc.CollectionChanged += value; }
			remove { if(Items is INotifyCollectionChanged ncc)ncc.CollectionChanged -= value; }
		}

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
            }
            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            isDisposed = true;
        }

        void ThrowExceptionIfDisposed() {
            if(isDisposed) throw new ObjectDisposedException(GetType().FullName,"The instance has already been disposed and cannot be operated on.");
        }

		/// <inheritdoc/>
		public IEnumerator<T> GetEnumerator() {
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable)Items).GetEnumerator();
		}

		/// <summary>
		/// 確保された、購読するプロパティ一覧を編集するためのコレクションを表す。同一の<c>Expression&lt;Func&lt;T, object&gt;&gt;</c>が含まれる場合は除外されます。
		/// </summary>
		public class ExpressionSubscriptionList : IEnumerable<Expression<Func<T,object>>>, IDisposable {
			Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>> _area = new();
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>,IEnumerable<Expression<Func<T, object>>>> _addAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>,IEnumerable<Expression<Func<T, object>>>> _removeAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _clearAction;
			Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> _dispAction;
			internal ExpressionSubscriptionList(
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> addAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>, IEnumerable<Expression<Func<T, object>>>> removeAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>> clearAction,
				Action<Dictionary<Expression<Func<T, object>>, Dictionary<ObservedPropertyTree<T>, IDisposable>>>  dispAction) {
				_addAction = addAction;
				_removeAction = removeAction;
				_clearAction = clearAction;
				_dispAction = dispAction;
			}
			/// <summary>購読するプロパティを追加する。同一の<c>Expression&lt;Func&lt;T, object&gt;&gt;</c>が含まれる場合は除外されます。</summary>
			/// <param name="expressions"></param>
			public void Add(IEnumerable<Expression<Func<T, object>>> expressions) {
				_addAction(_area,expressions);
			}
			/// <summary>指定したプロパティの購読を解除する。</summary>
			/// <param name="expressions"></param>
			public void Remove(IEnumerable<Expression<Func<T, object>>> expressions) { _removeAction(_area, expressions); }
			/// <summary>購読を全て解除する。</summary>
			public void Clear() { _clearAction(_area); }
			/// <summary>購読を全て解除し、確保されたコレクションを破棄する。</summary>
			public void Dispose() { _dispAction(_area); }
			/// <inheritdoc/>
			public IEnumerator<Expression<Func<T, object>>> GetEnumerator() { return _area.Keys.GetEnumerator(); }
			IEnumerator IEnumerable.GetEnumerator() { return _area.Keys.GetEnumerator(); }
		}
	}
}
