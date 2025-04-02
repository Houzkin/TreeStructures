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
using TreeStructures.Utilities;

namespace TreeStructures.Collections {
    /// <summary>Provides an imitable collection with synchronization.</summary>
    public abstract class ImitableCollection : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable {
        private protected readonly IEnumerable _source;
        private bool disposedValue;
        /// <summary>Initializes an instance.</summary>
        /// <param name="source">The source collection for synchronization.</param>
        protected ImitableCollection(IEnumerable source) { _source = source; }
        ///// <summary>Source collection </summary>
        //public IEnumerable Source => _source;
        /// <summary><inheritdoc/></summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary><inheritdoc/></summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        /// <summary>Raises property change notifications.</summary>
        /// <param name="e"></param>
        protected void RaisePropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }
        /// <summary>Raises collection change notifications.</summary>
        /// <param name="e"></param>
        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
            CollectionChanged?.Invoke(this, e);
        }
        #region static methods
        /// <summary>Assists in initializing an instance.</summary>
        /// <typeparam name="TSrc">The type of elements in the source collection for synchronization.</typeparam>
        /// <typeparam name="TConv">The type of elements in the target imitable collection.</typeparam>
        /// <param name="collection">The source collection for synchronization.</param>
        /// <param name="converter">A function to convert elements from <typeparamref name="TSrc"/> to corresponding <typeparamref name="TConv"/>.</param>
        /// <param name="removedAction">Action to be performed when an element is removed from the collection.</param>
        /// <param name="isImitate">>Specifies whether to initialize in a synchronized state.</param>
        /// <returns></returns>
        public static ImitableCollection<TSrc,TConv> Create<TSrc,TConv>(IEnumerable<TSrc> collection,Func<TSrc,TConv> converter,Action<TConv>? removedAction = null, bool isImitate = true) where TConv : class {
            Func<TSrc, ConvertPair<TSrc, TConv>> generator = x => {
                var m = x;
                return new ConvertPair<TSrc, TConv>(m, converter(m));
            };
            return new ImitableCollection<TSrc,TConv>(collection, generator, removedAction,isImitate);
        }
        #endregion

        #region Dispose
        /// <summary>Adds resource disposal in derived classes.</summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }
            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            disposedValue = true;
        }
        /// <summary>Disposes of the instance.</summary>
        public void Dispose() {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            if (disposedValue) return;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            
        }
        /// <summary>Prevents operations on an already disposed instance.</summary>
        /// <exception cref="ObjectDisposedException"></exception>
        protected void ThrowExceptionIfDisposed() {
            if(disposedValue) throw new ObjectDisposedException(GetType().FullName,"The instance has already been disposed and cannot be operated on.");
        }
        #endregion
        internal class ConvertPair<TSrc, TConv> {
            public ConvertPair(TSrc ele, TConv syncObj) {
                Before = ele; After = syncObj;
            }
            public TSrc Before { get; private set; }
            public TConv After { get; private set; }
        }
        //internal class ConvertPair<TSrc, TConv> : ConvertPair<TConv> where TConv : class {
        //    public ConvertPair(TSrc element, TConv syncObj) : base(element, syncObj) { }
        //    public new TSrc Before { get { return base.Before is null ? default : (TSrc)base.Before; } }
        //    //public new TSrc Before { get => base.Before is TSrc src1 ? src1 : default; }
        //    //public new TConv After { get { return base.After as TConv; } }
        //}
    }
    /// <summary><inheritdoc/></summary>
    /// <typeparam name="TConv">The type of elements in the imitable collection for synchronization.</typeparam>
    public abstract class ImitableCollection<TConv> : ImitableCollection,IReadOnlyList<TConv> where TConv : class {
        internal ImitableCollection(IEnumerable collection) : base(collection) { }

        /// <summary>Indicates whether the current state is in synchronization.</summary>
        public abstract bool IsImitating { get; }
        /// <summary>If not in synchronization state, starts synchronization.</summary>
        public abstract void Imitate();
        /// <summary>Stops synchronization and clears the imitable collection.</summary>
        public abstract void ClearAndPause();

		/// <summary>
		/// Gets this collection as a read-only <see cref="ReadOnlyObservableCollection{T}"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="ReadOnlyObservableCollection{T}"/> corresponding to this instance.
		/// The first call creates and caches the instance, and subsequent calls return the cached instance.
		/// </returns>
		/// <remarks>
		/// Wraps the internal <see cref="ObservableCollection{T}"/> in a <see cref="ReadOnlyObservableCollection{T}"/>.
		/// Any changes made to the original collection will be reflected in the read-only collection,
		/// but modifications to the read-only collection itself are not allowed.
		/// </remarks>
        public abstract ReadOnlyObservableCollection<TConv> AsReadOnlyObservableCollection();
        
        #region IReadOnlyList Members
        /// <inheritdoc/>
        public abstract TConv this[int index] { get; }

        /// <inheritdoc/>
        public abstract int Count { get; }

        /// <inheritdoc/>
        public abstract IEnumerator<TConv> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion
    }

    /// <inheritdoc/>
    /// <typeparam name="TSrc">The type of elements in the source collection for synchronization.</typeparam>
    /// <typeparam name="TConv">The type of elements in the target imitable collection.</typeparam>
    public class ImitableCollection<TSrc,TConv> : ImitableCollection<TConv>  where TConv : class {
        internal ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, ConvertPair<TSrc, TConv>> toSetConverter, Action<TConv>? removedAction = null, bool isImitate = true)
            : base(collection: collection) {
            _srcs = collection;
            _removedAction = removedAction;
            _isImitating = isImitate;
            _pairs = new List<ConvertPair<TSrc, TConv>>();
            _Aligner = new ListAligner<ConvertPair<TSrc, TConv>, TSrc, IList<ConvertPair<TSrc, TConv>>>(_pairs, toSetConverter, (cp, s) => object.Equals(cp.Before, s),
                insert:insertAction,replace:replaceAction,move:moveAction,remove:removeAction,clear:clearActon);
            if (_isImitating) SetReferenceWithStartObserve();
            else switchConnection(false);
        }
        /// <summary>Initializes a new instance.</summary>
        /// <param name="collection">The source collection for synchronization.</param>
        /// <param name="converter">A function to convert elements from <typeparamref name="TSrc"/> to corresponding <typeparamref name="TConv"/>.</param>
        /// <param name="removedAction">Action to be performed when an element is removed from the collection.</param>
        /// <param name="isImitate">>Specifies whether to initialize in a synchronized state.</param>
        public ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, TConv> converter, Action<TConv>? removedAction = null, bool isImitate = true)
            : this(collection, new Func<TSrc, ConvertPair<TSrc, TConv>>(src => new ConvertPair<TSrc, TConv>(src, converter(src))), removedAction,isImitate) {
        }

        IEnumerable<TSrc> _srcs;
        IList<ConvertPair<TSrc, TConv>> _pairs;
        ListAligner<ConvertPair<TSrc,TConv>, TSrc, IList<ConvertPair<TSrc,TConv>>> _Aligner;
        LumpedDisopsables Disposables = new LumpedDisopsables();
        Action<TConv>? _removedAction;
        bool _isImitating;

		#region edit list
		void insertAction(IList<ConvertPair<TSrc, TConv>> pairs,int index, ConvertPair<TSrc,TConv> item) {
            pairs.Insert(index, item);
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new TConv[1] { item.After }, index));
        }
        void replaceAction(IList<ConvertPair<TSrc, TConv>> pairs,int index, ConvertPair<TSrc,TConv> item) {
            var rmv = pairs[index].After;
            pairs[index] = item;
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new TConv[1] { item.After }, new TConv[1] { rmv }, index));
            _removedAction?.Invoke(rmv);
        }
        void removeAction(IList<ConvertPair<TSrc, TConv>> pairs,int index) {
            var rmv = pairs[index].After;
            pairs.RemoveAt(index);
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new TConv[1] { rmv }, index));
            _removedAction?.Invoke(rmv);
        }
        void moveAction(IList<ConvertPair<TSrc, TConv>> pairs,int ordIdx,int newIdx) {
            var mv = pairs[ordIdx];
            pairs.RemoveAt(ordIdx);
            pairs.Insert(newIdx, mv);
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, new TConv[1] { mv.After }, ordIdx, newIdx));
        }
        void clearActon(IList<ConvertPair<TSrc, TConv>> pairs) {
            var ary = pairs.Select(x => x.After).ToArray();
            pairs.Clear();
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            if(_removedAction != null) 
                foreach(var rmv in ary)
                    _removedAction.Invoke(rmv);
        }
        #endregion

        void Align() {
            _Aligner.AlignBy(_srcs);
        }
        void Clear() {
            _Aligner.AlignBy(Enumerable.Empty<TSrc>());
        }
        void SetReferenceWithStartObserve() {
            if(_source is INotifyPropertyChanged npc) {
                this.Disposables.Add(new EventListener<PropertyChangedEventHandler>(
                    h => npc.PropertyChanged += h,
                    h => npc.PropertyChanged -= h,
                    (s, e) => this.RaisePropertyChanged(e)));
            }
            if(_source is INotifyCollectionChanged ncc) {
                this.Disposables.Add(new EventListener<NotifyCollectionChangedEventHandler>(
                    h => ncc.CollectionChanged += h,
                    h => ncc.CollectionChanged -= h,
                    (s, e) => this.Align()));
            }
            this.Align();
        }
        bool switchConnection(bool observe) {
            if (_isImitating == observe) return false;
            _isImitating = observe;
            return true;
        }
        /// <inheritdoc/>
        public override bool IsImitating => _isImitating;
        /// <inheritdoc/>
		public override void Imitate() {
            if (!switchConnection(true)) return;
            ThrowExceptionIfDisposed();
            SetReferenceWithStartObserve();
		}
        /// <inheritdoc/>
		public override void ClearAndPause() {
            if (!switchConnection(false)) return;
            this.Disposables.Dispose();
            this.Clear();
		}
        /// <inheritdoc/>
		protected override void Dispose(bool disposing) {
            if (disposing) this.ClearAndPause();
			base.Dispose(disposing);
		}
        ReadOnlyObservableCollection<TConv>? _readOnlyColl;
        /// <inheritdoc/>
		public override ReadOnlyObservableCollection<TConv> AsReadOnlyObservableCollection() {
            return _readOnlyColl ??= new ReadOnlyObservableEnumerableWrapper<TConv,ImitableCollection<TConv>>(this);
		}
		#region ReadOnlyList Member
        /// <inheritdoc/>
		public override TConv this[int index] => this._pairs[index].After;
        /// <inheritdoc/>
        public override int Count => _pairs.Count;
        /// <inheritdoc/>
		public override IEnumerator<TConv> GetEnumerator() {
            return this._pairs.Select(x=>x.After).GetEnumerator();
		}
		#endregion
	}

}
