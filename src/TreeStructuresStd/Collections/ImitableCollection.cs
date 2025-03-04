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
using TreeStructures.EventManagement;
using TreeStructures.Internals;
using TreeStructures.Linq;

namespace TreeStructures.Collections {
    /// <summary>Provides an imitable collection with synchronization.</summary>
    public abstract class ImitableCollection : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable {
        private protected readonly IEnumerable _source;
        private bool disposedValue;
        /// <summary>Initializes an instance.</summary>
        /// <param name="source">The source collection for synchronization.</param>
        protected ImitableCollection(IEnumerable source) { _source = source; }
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
            if(disposedValue) throw new ObjectDisposedException(GetType().FullName,"既に破棄されたインスタンスが操作されました。");
        }
        #endregion
        internal class ConvertPair {
            public ConvertPair(object ele, object syncObj) {
                Before = ele; After = syncObj;
            }
            public object Before { get; private set; }
            public object After { get; private set; }
        }
        internal class ConvertPair<TSrc, TConv> : ConvertPair where TConv : class {
            public ConvertPair(TSrc element, TConv syncObj) : base(element, syncObj) { }
            public new TSrc Before { get { return base.Before is null ? default: (TSrc)base.Before; } }
            public new TConv After { get { return base.After as TConv; } }
        }
    }
    /// <summary><inheritdoc/></summary>
    /// <typeparam name="TConv">The type of elements in the imitable collection for synchronization.</typeparam>
    public class ImitableCollection<TConv> : ImitableCollection,IReadOnlyList<TConv> where TConv : class {
        LumpedDisopsables Disposables = new LumpedDisopsables();
        internal IList<ConvertPair> _references = new List<ConvertPair>();
        internal readonly Func<object, ConvertPair> _toSyncSet;
        internal Action<TConv>? _removeAction;
        private ObservableCollection<TConv>? _obsList;
        private ReadOnlyObservableCollection<TConv>? _roObsList;

        internal ImitableCollection(IEnumerable collection,Func<object,ConvertPair> toSetConverter,Action<TConv>? removedAction = null,bool isImitate = true): base(collection) {

            _toSyncSet = toSetConverter;
            _removeAction = removedAction;
            if (isImitate) {
                SetReferenceWithStartObserveCollection();
            } else {
                this.SwitchConnection(false);
            }
        }
        /// <summary>Initializes a new instance.</summary>
        /// <param name="collection">The source collection for synchronization.</param>
        /// <param name="converter">A function to convert elements from <see cref="object"/> to corresponding <typeparamref name="TConv"/>.</param>
        /// <param name="removedAction">Action to be performed when an element is removed from the collection.</param>
        /// <param name="isImitate">>Specifies whether to initialize in a synchronized state.</param>
        public ImitableCollection(IEnumerable collection,Func<object,TConv> converter,Action<TConv>? removedAction = null,bool isImitate = true) 
            : this(collection,new Func<object,ConvertPair>(src => new ConvertPair(src, converter(src))), removedAction, isImitate) { }

        void SetReferenceWithStartObserveCollection() {
            if(_source is INotifyPropertyChanged npc) {
                var dsp = new EventListener<PropertyChangedEventHandler>(
                    h => npc.PropertyChanged += h,
                    h => npc.PropertyChanged -= h,
                    (s, e) => this.RaisePropertyChanged(e));
                this.Disposables.Add(dsp);
            }
            if (_source is INotifyCollectionChanged ncc) {
                var listener = new EventListener<NotifyCollectionChangedEventHandler>(
                    h => ncc.CollectionChanged += h,
                    h => ncc.CollectionChanged -= h,
                    onCollectionChangedAction);
                this.Disposables.Add(listener);
            }
            foreach (var item in _source) _references.Add(_toSyncSet(item));
        }
        
        void onCollectionChangedAction(object? sender,NotifyCollectionChangedEventArgs e) {
            ThrowExceptionIfDisposed();
            //新規リストに、並べ替えられた要素を格納
            IList<ConvertPair> newSrc = new List<ConvertPair>();
            List<ConvertPair> adds = new();//追加される要素
            foreach(var s in _source) {
                var v = _references.FirstOrDefault(x => object.Equals(x.Before, s));
                if(v != null) {
                    newSrc.Add(v);
                    _references.Remove(v);
                } else {
                    var synset = _toSyncSet(s);
                    adds.Add(synset);
                    newSrc.Add(synset);
                }
            }
            var trash = _references;//削除される要素

            var allItem = newSrc.Concat(_references).ToArray();
            _references = newSrc;//ソート後の要素に置換

            Func<IList?, IEnumerable<ConvertPair>, IList> getSyncObj = (tgt, range) => {
                return tgt == null ? Array.Empty<object>() : 
                range.Where(x => tgt.OfType<object?>().Any(y => object.Equals(y, x.Before))).Select(x => x.After).ToArray();
            };
            NotifyCollectionChangedEventArgs arg = e.Action switch {
                NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(e.Action, getSyncObj(e.NewItems,adds), e.NewStartingIndex),
                NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(e.Action,getSyncObj(e.OldItems,trash),e.OldStartingIndex),
                NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(e.Action,getSyncObj(e.NewItems,allItem), e.NewStartingIndex,e.OldStartingIndex),
                NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(e.Action, getSyncObj(e.NewItems, adds), getSyncObj(e.OldItems, trash), e.NewStartingIndex),
                NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(e.Action),
                _=>throw new ArgumentException(null, nameof(e)),
            };
            this.RaiseCollectionChanged(arg);
            if(_removeAction != null) {
                foreach(var th in trash.Select(x => x.After).OfType<TConv>()) {
                    _removeAction.Invoke(th);
                }
            }
            _obsList?.AlignBy(_references.Select(x => x.After).OfType<TConv>(),Equality<TConv>.ReferenceComparer);
        }
        /// <summary></summary>
        /// <returns></returns>
        public ReadOnlyObservableCollection<TConv> AsReadOnlyObservableCollection(){
            if(_roObsList is null || _obsList is null){
                if(_obsList is null){
                    _obsList = new ObservableCollection<TConv>();
                    _obsList?.AlignBy(_references.Select(x => x.After).OfType<TConv>(),Equality<TConv>.ReferenceComparer);
                }
                _roObsList ??= new ReadOnlyObservableCollection<TConv>(_obsList!);
            }
            return _roObsList;
        }
        /// <summary><inheritdoc/></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing) {
            if(disposing) { this.PauseImitateAndClear(); }
            base.Dispose(disposing);
        }
        /// <summary>Indicates whether the current state is in synchronization.</summary>
        public bool IsImitating { get; private set; } = true;
        bool SwitchConnection(bool observe) {
            if (IsImitating == observe) return false;
            IsImitating = observe;
            return true;
        }
        /// <summary>If not in synchronization state, starts synchronization.</summary>
        public void Imitate() {
            if (!SwitchConnection(true)) return;
            SetReferenceWithStartObserveCollection();
            var ary = _references.Select(x => x.After).ToArray();
            if (ary.Any()) {
                for(int i=0; i < ary.Length;i++) {
                    this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ary[i], i));
                }
            }
            _obsList?.AlignBy(_references.Select(x => x.After).OfType<TConv>(),Equality<TConv>.ReferenceComparer);
        }
        /// <summary>Stops synchronization and clears the imitable collection.</summary>
        public void PauseImitateAndClear() {
            if(!SwitchConnection(false)) return;
            this.Disposables.Dispose();
            _references.Clear();
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _obsList?.AlignBy(_references.Select(x => x.After).OfType<TConv>(), Equality<TConv>.ReferenceComparer);
        }
        
        #region IReadOnlyList Members
        /// <summary><inheritdoc/></summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TConv this[int index] => _references[index].After as TConv;

        /// <summary><inheritdoc/></summary>
        public int Count => _references.Count;

        /// <summary><inheritdoc/></summary>
        public IEnumerator<TConv> GetEnumerator() => _references.Select(x=>x.After as TConv).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _references.Select(x => x.After).GetEnumerator();
        #endregion
    }

    /// <inheritdoc/>
    /// <typeparam name="TSrc">The type of elements in the source collection for synchronization.</typeparam>
    /// <typeparam name="TConv">The type of elements in the target imitable collection.</typeparam>
    public class ImitableCollection<TSrc,TConv> : ImitableCollection<TConv>  where TConv : class {
        internal ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, ConvertPair<TSrc, TConv>> toSetConverter, Action<TConv>? removedAction = null, bool isImitate = true)
            : base(collection: collection, toSetConverter: src => toSetConverter((TSrc)src), removedAction: removedAction,isImitate: isImitate) {


        }
        /// <summary>Initializes a new instance.</summary>
        /// <param name="collection">The source collection for synchronization.</param>
        /// <param name="converter">A function to convert elements from <typeparamref name="TSrc"/> to corresponding <typeparamref name="TConv"/>.</param>
        /// <param name="removedAction">Action to be performed when an element is removed from the collection.</param>
        /// <param name="isImitate">>Specifies whether to initialize in a synchronized state.</param>
        public ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, TConv> converter, Action<TConv>? removedAction = null, bool isImitate = true)
            : this(collection, new Func<TSrc, ConvertPair<TSrc, TConv>>(src => new ConvertPair<TSrc, TConv>(src, converter(src))), removedAction,isImitate) { }
       
        
    }
    
}
