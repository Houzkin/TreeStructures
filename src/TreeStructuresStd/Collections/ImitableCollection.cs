using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using TreeStructures.Events;
using TreeStructures.Linq;
using TreeStructures.Utilities;

namespace TreeStructures.Collections {

	/// <inheritdoc/>
	/// <typeparam name="TSrc">The type of elements in the source collection for synchronization.</typeparam>
	/// <typeparam name="TDst">The type of elements in the target imitable collection.</typeparam>
	public class ImitableCollection<TSrc, TDst> : ImitableCollection<TDst> {

		/// <summary>Initializes a new instance.</summary>
		/// <param name="collection">The source collection for synchronization.</param>
		/// <param name="convert">A function to convert elements from <typeparamref name="TSrc"/> to corresponding <typeparamref name="TDst"/>.</param>
		/// <param name="removedAction">Action to be performed when an element is removed from the collection.</param>
		/// <param name="isImitate">Specifies whether to initialize in a synchronized state.</param>
		public ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, TDst> convert, Action<TDst>? removedAction = null, bool isImitate = true)
			: base() {

			_SDPairList = new SDPairCollection(
				collection,
				new(src => new SDPair(src, convert(src))),
				(s, d) => Equality<TSrc>.ValueOrReferenceComparer.Equals(s, d.Src),
				a => removedAction?.Invoke(a.Dst),
				isImitate);

			((INotifyCollectionChanged)_SDPairList).CollectionChanged += (s, e) => OnCollectionChanged(ArgsConvert(e));
			((INotifyPropertyChanged)_SDPairList).PropertyChanged += (s, e) => OnPropertyChanged(e);
		}
		SDPairCollection _SDPairList;
		//ReadOnlyObservableCollection<TDst>? _readOnly;

		private NotifyCollectionChangedEventArgs ArgsConvert(NotifyCollectionChangedEventArgs e) {
			Func<SDPair,TDst> toDst = a => a.Dst;
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add:
				var newItems = e.NewItems?
					.OfType<SDPair>()
					.Select(item => toDst(item))
					.ToList();
				return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, e.NewStartingIndex);

			case NotifyCollectionChangedAction.Remove:
				var oldItems = e.OldItems?
					.OfType<SDPair>()
					.Select(item => toDst(item))
					.ToList();
				return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, e.OldStartingIndex);

			case NotifyCollectionChangedAction.Replace:
				var replacedNewItems = e.NewItems?
					.OfType<SDPair>()
					.Select(item => toDst(item))
					.ToList();
				var replacedOldItems = e.OldItems?
					.OfType<SDPair>()
					.Select(item => toDst(item))
					.ToList();
				return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacedNewItems, replacedOldItems, e.NewStartingIndex);

			case NotifyCollectionChangedAction.Move:
				var movedItems = e.NewItems?
					.OfType<SDPair>()
					.Select(item => toDst(item))
					.ToList();
				return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, movedItems, e.NewStartingIndex, e.OldStartingIndex);

			case NotifyCollectionChangedAction.Reset:
				return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

			default:
				throw new NotSupportedException($"Unknown collection change action: {e.Action}");
			}
		}

		/// <inheritdoc/>
		public override bool IsImitating => _SDPairList.IsImitating;
		/// <inheritdoc/>
		public override void Imitate() {
			ThrowExceptionIfDisposed();
			_SDPairList.Start();
		}
		/// <inheritdoc/>
		public override void Pause() {
			_SDPairList.Stop();
		}
		/// <inheritdoc/>
		public override void ClearAndPause() {
			_SDPairList.ClearAndStop();
		}
		/// <inheritdoc/>
		protected override void Dispose(bool disposing) {
			if (disposing) this.ClearAndPause();
			base.Dispose(disposing);
		}
		///// <inheritdoc/>
		//public override ReadOnlyObservableCollection<TDst> AsReadOnlyObservableCollection() {
		//	return _readOnly ??= new ReadOnlyObservableProxyCollection<SDPair, TDst>(_SDPairList, x => x.Dst, (p, d) => Equality<TDst>.ValueOrReferenceComparer.Equals(p.Dst, d));// EqualityComparer<TDst>.Default.Equals(p.Dst, d));
		//}
		#region ReadOnlyMembers

		/// <inheritdoc/>
		public override TDst this[int index] => _SDPairList[index].Dst;
		/// <inheritdoc/>
		public override int Count => _SDPairList.Count;
		/// <inheritdoc/>
		public override IEnumerator<TDst> GetEnumerator() {
			return this._SDPairList.Select(x => x.Dst).GetEnumerator();
		}
		#endregion
		private class SDPair {
			public SDPair(TSrc src, TDst dst) {
				Src = src;
				Dst = dst;
			}
			public TSrc Src { get; private set; }
			public TDst Dst { get; private set; }
		}
		private class SDPairCollection: ReadOnlyObservableProxyCollection<TSrc, SDPair> {
			public SDPairCollection(IEnumerable<TSrc> source, Func<TSrc, SDPair> convert, Func<TSrc, SDPair, bool> equality, Action<SDPair>? removedAction = null, bool isImitating = true)
				: base(source, convert, equality, removedAction) {
				_src = source;
				_alignItems = () => Items.AlignBy(this.SourceItems, convert, equality);
				_clearItems = () => {
					var lst = removedAction != null ? Items.ToArray() : Array.Empty<SDPair>();
					Items.Clear();
					foreach (var rm in lst) removedAction?.Invoke(rm);
				};
				if (isImitating) Start();
			}
			IEnumerable<TSrc> _src;
			Action _alignItems;
			Action _clearItems;
			bool _isImitating = false;
			LumpedDisopsables _disposables = new LumpedDisopsables();
			protected override IEnumerable<TSrc> SourceItems => _src;
			bool switchConnection(bool imitate) {
				if (_isImitating == imitate) return false;
				_isImitating = imitate;
				this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsImitating)));
				return true;
			}
			public bool IsImitating => _isImitating;
			public void Start() {
				if (!switchConnection(true)) {
					if(SourceItems is not INotifyCollectionChanged) _alignItems?.Invoke();
					return;
				}
				if (SourceItems is INotifyCollectionChanged ncc) {
					this._disposables.Add(new EventListener<NotifyCollectionChangedEventHandler>(
						h => ncc.CollectionChanged += h,
						h => ncc.CollectionChanged -= h,
						(s, e) => this.ApplyCollectionChange(e)));
				}
				_alignItems?.Invoke();
			}
			public void ClearAndStop() {
				_clearItems?.Invoke();
				switchConnection(false);
				_disposables.Dispose();
			}
			public void Stop() {
				switchConnection(false);
				_disposables.Dispose();
			}
		}
	}
}
