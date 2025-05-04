using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;
using TreeStructures.Events;
using TreeStructures.Utilities;
using TreeStructures.Linq;

namespace TreeStructures.Collections {

	/// <summary>
	/// Provides an observable and combinable collection.  
	/// To reflect changes in the combined collections, the target collections must implement <see cref="INotifyCollectionChanged"/>.
	/// After use, call <see cref="ReadOnlyObservableProxyCollection{T,U}.Dispose()"/> to unsubscribe from the collection change notifications.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	public class ObservableCombinableCollection<T> : ReadOnlyObservableProxyCollection<T> {
		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableCombinableCollection{T}"/> class.
		/// </summary>
		/// <param name="addingAction">An optional callback invoked just prior to adding an item to the collection.</param>
		/// <param name="removedAction">An optional callback invoked when an item is removed.</param>
		/// <param name="equality">An optional equality comparer used to detect duplicate items.</param>
		public ObservableCombinableCollection(Action<T>? addingAction = null, Action<T>? removedAction = null, IEqualityComparer<T>? equality = null) 
			: base(Enumerable.Empty<T>(), addingAction, removedAction, equality) { }

		List<Tuple<IEnumerable,ImitableCollection<T>,IDisposable>> _combines = new();
		bool attach(int index, IEnumerable<T> collection) {
			ThrowExceptionIfDisposed();
			if (_combines.Any(x => x.Item1 == collection)) return false;
			//keyとなるIEnumerableからImitableCollectionを生成
			var imit = collection.ToImitable(x => x, null, false);
			//imitのイベントを購読
			var lsnr = new EventListener<NotifyCollectionChangedEventHandler>(
				h => ((INotifyCollectionChanged)imit).CollectionChanged +=h,
				h=> ((INotifyCollectionChanged)imit).CollectionChanged -=h,
				(s, e) => changedAction(s,e));
			//除外時の処理
			var dsp = new DisposableObject(() => {
				imit.Dispose();
				lsnr.Dispose();
			});
			//keyとなるIEnumerable,imit,除外時の処理(IDisposable)をまとめて追加
			_combines.Insert(index, new Tuple<IEnumerable, ImitableCollection<T>, IDisposable>(collection, imit, dsp));

			imit.Imitate();
			return true;
		}
		/// <summary>
		/// Gets the source collection to observe for change notifications.
		/// </summary>
		protected override IEnumerable<T> SourceItems => _combines.SelectMany(x => x.Item2);//base.SourceItems;
		bool detach(IEnumerable collection) {
			ThrowExceptionIfDisposed();
			var tgt =_combines.FirstOrDefault(x => x.Item1 == collection);
			if(tgt == null) return false;
			tgt.Item3.Dispose();
			_combines.Remove(tgt);
			return true; 
		}
		void changedAction(object? s,NotifyCollectionChangedEventArgs e) {
			if (s is not ImitableCollection<T> || s is null) return;
			var imit  = s as ImitableCollection<T>;
			//baseIdx + curIdx = コレクション全体におけるindexとなる
			var curTpl = _combines.Select((Tpl, Idx) => new { Tpl, Idx }).First(x => x.Tpl.Item2 == imit);
			var baseIdx = _combines.Take(curTpl.Idx).SelectMany(x => x.Item2).Count();
			this.ApplyCollectionChange(adjEventArgs(e, baseIdx));
		}
		NotifyCollectionChangedEventArgs adjEventArgs(NotifyCollectionChangedEventArgs e,int baseIdx) {
			Func<int, int> toInnerIndex = eachIdx => eachIdx < 0 ? eachIdx : baseIdx + eachIdx;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				var idx = toInnerIndex(e.NewStartingIndex);
				return new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, toInnerIndex(e.NewStartingIndex));
			case NotifyCollectionChangedAction.Remove:
				return new NotifyCollectionChangedEventArgs(e.Action,e.OldItems, toInnerIndex(e.OldStartingIndex));
			case NotifyCollectionChangedAction.Replace:
				return new NotifyCollectionChangedEventArgs(e.Action,e.NewItems,e.OldItems,toInnerIndex(e.NewStartingIndex));
			case NotifyCollectionChangedAction.Move:
				return new NotifyCollectionChangedEventArgs(e.Action,e.NewItems,toInnerIndex(e.NewStartingIndex),toInnerIndex(e.OldStartingIndex));
			case NotifyCollectionChangedAction.Reset:
				var rmvCnt = Items.Count - _combines.SelectMany(x => x.Item2).Count();
				var rmvItms= Items.Skip(baseIdx).Take(rmvCnt);
				return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, rmvItms.ToList(), baseIdx);
			default:
				throw new NotSupportedException($"Unknown collection change action: {e.Action}");
			}
		}
		/// <summary>
		/// Appends a new collection to be observed.
		/// </summary>
		/// <param name="collection">The collection to observe and append.</param>
		/// <returns><c>true</c> if the collection was successfully appended; otherwise, <c>false</c>.</returns>
		public bool AppendCollection(IEnumerable<T> collection) {
			return attach(_combines.Count, collection);
		}
		/// <summary>
		/// Inserts a new collection at the specified index to be observed.
		/// </summary>
		/// <param name="index">The index at which the collection should be inserted.</param>
		/// <param name="collection">The collection to observe and insert.</param>
		/// <returns><c>true</c> if the collection was successfully inserted; otherwise, <c>false</c>.</returns>
		public bool InsertCollection(int index, IEnumerable<T> collection) {
			return attach(index, collection);
		}
		/// <summary>
		/// Removes the specified collection from observation.
		/// </summary>
		/// <param name="collection">The collection to remove.</param>
		/// <returns><c>true</c> if the collection was found and removed; otherwise, <c>false</c>.</returns>
		public bool RemoveCollection(IEnumerable<T> collection) {
			return detach(collection);
		}
		/// <summary>
		/// Clears all combined collections.
		/// </summary>
		public void ClearCollections() {
			foreach (var col in _combines.AsEnumerable().Reverse()) {
				detach(col.Item1);
			}
		}
		/// <inheritdoc/>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				_combines.Select(x => x.Item3).CombineDisposables().Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
