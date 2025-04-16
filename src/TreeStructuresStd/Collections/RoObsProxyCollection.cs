using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using TreeStructures.Events;
using TreeStructures.Linq;

namespace TreeStructures.Collections {
	/// <summary>
	/// A read-only Observable collection that monitors changes in the specified collection and provides transformed elements.<br/>
	/// To reflect changes in the collection, the specified collection must implement <see cref="INotifyCollectionChanged"/>.<br/>
	/// After use, call <see cref="Dispose()"/> to unsubscribe from the collection change notifications.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the source collection.</typeparam>
	/// <typeparam name="U">The type of the elements in the wrapped collection that are linked to <typeparamref name="T"/>.</typeparam>
	public class ReadOnlyObservableProxyCollection<T, U> : ReadOnlyObservableCollection<U>, IDisposable {
		private bool isDisposed;
		private IEnumerable<T> _collection;
		IDisposable? _collectionChangedListener;
		Func<T, U> _converter;
		Func<T, U, bool> _equality;
		Action<U>? _removedHandler;

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="collection">The source collection that implements <see cref="INotifyCollectionChanged"/>.</param>
		/// <param name="convert">A function that converts <typeparamref name="T"/> elements to <typeparamref name="U"/>.</param>
		/// <param name="equality">A function that determines whether a given <typeparamref name="T"/> element corresponds to a given <typeparamref name="U"/> element.</param>
		/// <param name="removedHandler">An optional callback invoked when an item is removed.</param>
		public ReadOnlyObservableProxyCollection(IEnumerable<T> collection, Func<T, U> convert, Func<T, U, bool> equality, Action<U>? removedHandler = null)
			: base(new ObservableCollection<U>()) {
			_collection = collection;
			_converter = convert;
			_equality = equality;
			_removedHandler = removedHandler;
			if (SourceItems is INotifyCollectionChanged ncc) {
				_collectionChangedListener = new EventListener<NotifyCollectionChangedEventHandler>(
					h => ncc.CollectionChanged += h,
					h => ncc.CollectionChanged -= h,
					(s, e) => ApplyCollectionChange(e));
				//(s,e)=>_SourceCollectionChanged(s,e));
			}
			Items.AlignBy(SourceItems ?? Enumerable.Empty<T>(), _converter, _equality);
		}
		/// <summary>
		/// Returns the <see cref="ObservableCollection{U}"/> that the <see cref="ReadOnlyObservableCollection{U}"/> wraps.
		/// </summary>
		protected new ObservableCollection<U> Items => (ObservableCollection<U>)base.Items;
		/// <summary>
		/// Gets the source collection to observe for change notifications.
		/// </summary>
		/// <remarks>
		/// By default, this returns the collection passed to the constructor.  <br/>
		/// When overridden, the returned collection will not be subscribed to during construction.  <br/>
		/// In such cases, the derived class is responsible for managing change subscriptions and initial synchronization of <see cref="Items"/>.
		/// </remarks>
		protected virtual IEnumerable<T> SourceItems => _collection;

		/// <summary>
		/// Applies the specified collection change event to the internal <see cref="Items"/> collection,
		/// synchronizing it with the source collection by adding, removing, replacing, or moving items as needed.
		/// </summary>
		/// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> that describes the change in the source collection.</param>
		/// <remarks>
		/// This method is typically called when the source collection raises a <see cref="INotifyCollectionChanged.CollectionChanged"/> event.
		/// It ensures that <see cref="Items"/> reflects the same changes, using the provided conversion and equality functions.
		/// </remarks>
		protected virtual void ApplyCollectionChange(NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				for (int i = 0; i < e.NewItems?.Count; i++) {
					var newItem = (T?)e.NewItems[i];
					int insertIndex = e.NewStartingIndex >= 0 ? e.NewStartingIndex + i : Items.Count;
					Items.Insert(insertIndex, _converter(newItem));
				}
				break;

			case NotifyCollectionChangedAction.Remove:
				// RemoveAtで削除する方法（推奨: 一致の保証がある場合）
				if (e.OldStartingIndex >= 0) {
					for (int i = 0; i < e.OldItems?.Count; i++) {
						var rm = Items[e.OldStartingIndex];
						Items.RemoveAt(e.OldStartingIndex);
						_removedHandler?.Invoke(rm);
					}
				} else {  // indexがない場合は内容ベースで削除
					for (int i = 0; i < e.OldItems?.Count; i++) {
						var oldItem = (T?)e.OldItems[i];
						var rm = Items.First(x => _equality(oldItem, x));
						Items.Remove(rm);
						_removedHandler?.Invoke(rm);
					}
				}
				break;

			case NotifyCollectionChangedAction.Replace:
				for (int i = 0; i < e.NewItems?.Count; i++) {
					var newItem = (T?)e.NewItems[i];
					int replaceIndex = e.NewStartingIndex + i;
					var rm = Items[replaceIndex];
					Items[replaceIndex] = _converter(newItem);
					_removedHandler?.Invoke(rm);
				}
				break;

			case NotifyCollectionChangedAction.Move:
				for (int i = 0; i < e.NewItems?.Count; i++) {
					int oldIndex = e.OldStartingIndex + i;
					int newIndex = e.NewStartingIndex + i;
					if (oldIndex == newIndex) continue;

					Items.Move(oldIndex, newIndex);
				}
				break;

			case NotifyCollectionChangedAction.Reset:
				var lst = _removedHandler != null ? Items.ToArray() : Array.Empty<U>();
				Items.Clear();
				foreach (var rm in lst)
					_removedHandler?.Invoke(rm);
				break;

			default:
				throw new NotSupportedException($"Unknown collection change action: {e.Action}");
			}

		}

		/// <summary>Prevents operations on an already disposed instance.</summary>
		/// <exception cref="ObjectDisposedException"></exception>
		protected void ThrowExceptionIfDisposed() {
			if (isDisposed) throw new ObjectDisposedException(GetType().FullName, "The instance has already been disposed and cannot be operated on.");
		}
		/// <summary>Releases the resources used by the collection.</summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (!isDisposed) {
				if (disposing) {
					// TODO: マネージド状態を破棄します (マネージド オブジェクト)
					_collectionChangedListener?.Dispose();
				}
				// TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
				// TODO: 大きなフィールドを null に設定します
				isDisposed = true;
			}
		}
		// // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
		// ~ReadOnlyObservableEnumerable()
		// {
		//     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
		//     Dispose(disposing: false);
		// }
		/// <inheritdoc/>
		public void Dispose() {
			// このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
	/// <summary>
	/// A read-only, observable collection that follows changes in a specified source collection.<br/>
	/// To reflect changes, the source collection must implement <see cref="INotifyCollectionChanged"/>.<br/>
	/// After use, call <see cref="ReadOnlyObservableProxyCollection{T, U}.Dispose()"/> to unsubscribe from change notifications.
	/// </summary>
	/// <typeparam name="T">The type of elements in the collection.</typeparam>
	public class ReadOnlyObservableProxyCollection<T> : ReadOnlyObservableProxyCollection<T, T> {
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadOnlyObservableProxyCollection{T}"/> class.
		/// </summary>
		/// <param name="source">The source collection to wrap.</param>
		/// <param name="addAction">An optional callback invoked just prior to adding an item to the collection.</param>
		/// <param name="removedAction">An optional callback invoked when an item is removed.</param>
		/// <param name="equality">An optional equality comparer used to determine element equivalence. If null, <see cref="Equality{T}.ValueOrReferenceComparer"/> is used.</param>
		public ReadOnlyObservableProxyCollection(IEnumerable<T> source, Action<T>? addAction = null, Action<T>? removedAction = null, IEqualityComparer<T>? equality = null)
			: base(source,
				  addAction is null ? (x => x) : (x => { addAction.Invoke(x); return x; }),
				  (x, y) => equality?.Equals(x, y) ?? Equality<T>.ValueOrReferenceComparer.Equals(x, y),
				  removedAction) {
		}
	}
}
