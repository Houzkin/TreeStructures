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
using TreeStructure.EventManager;
using TreeStructure.Internals;
using TreeStructure.Linq;

namespace TreeStructure.Collections {
    
    public abstract class ConvertedCollection : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable {
        protected readonly IEnumerable<object> _source;
        private bool disposedValue;

        public ConvertedCollection(IEnumerable<object> source) { _source = source; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;
        protected void RaisePropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }
        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
            CollectionChanged?.Invoke(this, e);
        }
        #region static methods
        public static ConvertedCollection<TSrc,TConv> Create<TSrc,TConv>(IEnumerable<TSrc> collection,Func<TSrc,TConv> converter,Action<TConv> removedAction) where TConv : class  where TSrc : class{
            Func<object, ConvertPair<TSrc, TConv>> generator = x => {
                var m = x as TSrc;
                return new ConvertPair<TSrc, TConv>(m, converter(m));
            };
            return new ConvertedCollection<TSrc,TConv>(collection, generator, removedAction);
        }
        #endregion

        #region Dispose
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }
            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            disposedValue = true;
        }

        public void Dispose() {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            if(!disposedValue) {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
        protected void ThrowExceptionIfDisposed() {
            if(disposedValue) throw new ObjectDisposedException(GetType().FullName,"既に破棄されたインスタンスが操作されました。");
        }
        #endregion
        internal class ConvertPair {
            public ConvertPair(object ele, object syncObj) {
                Before = ele; After = syncObj;
            }
            public object Before { get; init; }
            public object After { get; init; }
        }
        internal class ConvertPair<TSrc, TConv> : ConvertPair where TSrc : class where TConv : class {
            public ConvertPair(TSrc element, TConv syncObj) : base(element, syncObj) { }
            public new TSrc Before { get { return base.Before as TSrc; } }
            public new TConv After { get { return base.After as TConv; } }
        }
    }
    public class ConvertedCollection<TConv> : ConvertedCollection,IReadOnlyList<TConv> where TConv : class {
        LumpedDisopsables Disposables = new LumpedDisopsables();
        internal IList<ConvertPair> _references = new List<ConvertPair>();
        internal readonly Func<object, ConvertPair> _toSyncSet;
        //IDisposable _listListener;
        internal Action<TConv> _removeAction;
        //readonly Func<object, IDisposable> _getSrcObjListener;
        //ObservableCollection<TConv> _convedlist = new();
        internal ConvertedCollection(IEnumerable<object> collection,Func<object,ConvertPair> toSetConverter,Action<TConv> removedAction): base(collection) {

            _toSyncSet = toSetConverter;
            _removeAction = removedAction;
            SetReferenceWithStartObserveCollection();
        }
        public ConvertedCollection(IEnumerable<object> collection,Func<object,TConv> converter,Action<TConv> removedAction) 
            : this(collection,new Func<object,ConvertPair>(src => new ConvertPair(src, converter(src))), removedAction) { }

        void SetReferenceWithStartObserveCollection() {
            var ps = _source as INotifyPropertyChanged;
            if(ps != null) {
                var dsp = new EventListener<PropertyChangedEventHandler>(
                    h => ps.PropertyChanged += h,
                    h => ps.PropertyChanged -= h,
                    (s, e) => this.RaisePropertyChanged(e));
                this.Disposables.Add(dsp);
            }
            var col = _source as INotifyCollectionChanged;
            if(col == null) throw new ArgumentException("INotifyCollectionChanged インターフェイスを実装していません。");
            var listener = new EventListener<NotifyCollectionChangedEventHandler>(
                h => col.CollectionChanged += h,
                h => col.CollectionChanged -= h,
                onCollectionChangedAction);
            this.Disposables.Add(listener);

            foreach (var item in _source) _references.Add(_toSyncSet(item));
        }
        
        void onCollectionChangedAction(object? sender,NotifyCollectionChangedEventArgs e) {
            ThrowExceptionIfDisposed();
            //新規リストに、並べ替えられた要素を格納
            IList<ConvertPair> newSrc = new List<ConvertPair>();
            List<ConvertPair> adds = new();//追加される要素
            foreach(var s in _source) {
                var v = _references.FirstOrDefault(x => object.ReferenceEquals(x.Before, s));
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
            //var newVm = e.NewItems == null ? Array.Empty<object>() :
            //    adds.Where(x => e.NewItems.OfType<object>().Any(y => object.ReferenceEquals(y, x.Element))).Select(x => x.SyncObj).ToArray();
            //var oldVm = e.OldItems == null ? Array.Empty<object>() :
            //    trash.Where(x => e.OldItems.OfType<object>().Any(y => object.ReferenceEquals(y, x.Element))).Select(x => x.SyncObj).ToArray();
            _references = newSrc;//ソート後の要素に置換

            Func<IList?, IEnumerable<ConvertPair>, IList> getSyncObj = (tgt, range) => {
                return tgt == null ? Array.Empty<object>() : 
                range.Where(x => tgt.OfType<object?>().Any(y => object.ReferenceEquals(y, x.Before))).Select(x => x.After).ToArray();
            };
            NotifyCollectionChangedEventArgs arg = e.Action switch {
                NotifyCollectionChangedAction.Add => new NotifyCollectionChangedEventArgs(e.Action, getSyncObj(e.NewItems,adds), e.NewStartingIndex),
                NotifyCollectionChangedAction.Remove => new NotifyCollectionChangedEventArgs(e.Action,getSyncObj(e.OldItems,trash),e.OldStartingIndex),
                NotifyCollectionChangedAction.Move => new NotifyCollectionChangedEventArgs(e.Action,getSyncObj(e.NewItems,allItem), e.NewStartingIndex,e.OldStartingIndex),
                NotifyCollectionChangedAction.Replace => new NotifyCollectionChangedEventArgs(e.Action, getSyncObj(e.NewItems, adds), getSyncObj(e.OldItems, trash), e.NewStartingIndex),
                NotifyCollectionChangedAction.Reset => new NotifyCollectionChangedEventArgs(e.Action),
                _=>throw new ArgumentException(nameof(e)),
            };
            this.RaiseCollectionChanged(arg);
            if(_removeAction != null) {
                foreach(var th in trash.Select(x => x.After).OfType<TConv>()) {
                    _removeAction(th);
                }
            }
        }
        protected override void Dispose(bool disposing) {
            if(disposing) { this.Disposables.Dispose(); }
            base.Dispose(disposing);
        }
        public bool IsObserving { get; private set; } = true;
        bool TryChangeIsObservingCollection(bool observe) {
            if (IsObserving == observe) return false;
            IsObserving = observe;
            return true;
        }
        public void StartListening() {
            if (!TryChangeIsObservingCollection(true)) return;
            SetReferenceWithStartObserveCollection();
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _references.Select(x => x.After).ToArray(), 0));
        }
        public void StopListeningWithClear() {
            if(!TryChangeIsObservingCollection(false)) return;
            this.Disposables.Dispose();
            _references.Clear();
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        
        #region IReadOnlyList Members
        public TConv this[int index] => _references[index].After as TConv;
        public int Count => _references.Count;
        public IEnumerator<TConv> GetEnumerator() => _references.Select(x=>x.After as TConv).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _references.Select(x => x.After).GetEnumerator();
        #endregion
    }
    public class ConvertedCollection<TSrc,TConv> : ConvertedCollection<TConv> where TSrc :class where TConv : class {
        static Dictionary<string, Func<TSrc, bool>> dicFtr = new();
        static Dictionary<string,Func<TSrc,object>> dicCpr = new();
        string _currentFilter = string.Empty;
        string _currentComparer = string.Empty;
        //IComparer<TConv> _comparer;
        //ObservableCollection<TConv> _convedlist = new();
        internal ConvertedCollection(IEnumerable<TSrc> collection, Func<TSrc, ConvertPair<TSrc, TConv>> toSetConverter, Action<TConv> removedAction)
            : base(collection: collection, toSetConverter: src => toSetConverter(src as TSrc), removedAction: removedAction) {


        }
        public ConvertedCollection(IEnumerable<TSrc> collection, Func<TSrc, TConv> converter, Action<TConv> removedAction)
            : this(collection, new Func<TSrc, ConvertPair<TSrc, TConv>>(src => new ConvertPair<TSrc, TConv>(src, converter(src))), removedAction) { }
        //public ConvertedCollection(IEnumerable<TSrc> collection, Func<TSrc?, SynchroSet<TSrc, TConv>> toSetConverter, Action<TConv> removedAction,
        //    Expression<Func<TSrc, bool>> filter, Expression<Func<TSrc, object>> compiar, IComparer<object> comparer)
        //    : base(collection: collection, toSetConverter: src => toSetConverter(src as TSrc),removedAction:removedAction) {
        //}

        //public override event NotifyCollectionChangedEventHandler? CollectionChanged; {
        //    add { _convedlist.CollectionChanged += value; }
        //    remove { _convedlist.CollectionChanged -= value; }
        //}

        //IList<SynchroSet> SourceToReference(NotifyCollectionChangedEventArgs e) {
        //    List<SynchroSet> bfr = _references.ToList();
        //    List<SynchroSet> rmds = new();
        //    switch (e.Action) {//sourceにnullが複数あった場合、どれが変更されたかわからないためChangedActionで編集
        //    case NotifyCollectionChangedAction.Add when e.NewItems != null:
        //        bfr.Insert(e.NewStartingIndex, _toSyncSet(e.NewItems[0]));
        //        break;
        //    case NotifyCollectionChangedAction.Remove:
        //        rmds.Add(bfr[e.OldStartingIndex]);
        //        bfr.RemoveAt(e.OldStartingIndex);
        //        break;
        //    case NotifyCollectionChangedAction.Replace when e.NewItems != null:
        //        rmds.Add(bfr[e.NewStartingIndex]);
        //        bfr[e.NewStartingIndex] = _toSyncSet(e.NewItems[0]);
        //        break;
        //    case NotifyCollectionChangedAction.Move:
        //        var tmp = bfr[e.OldStartingIndex];
        //        bfr.RemoveAt(e.OldStartingIndex);
        //        bfr.Insert(e.NewStartingIndex, tmp);
        //        break;
        //    case NotifyCollectionChangedAction.Reset:
        //        rmds.AddRange(bfr);
        //        bfr.Clear();
        //        break;
        //    }
        //    if (_source.SequenceEqual(bfr.Select(x => x.Element))) {
        //        _references = bfr;
        //        return rmds;
        //    } else {
        //        var ns = new List<SynchroSet>();
        //        foreach (var s in _source) {
        //            var v = _references.FirstOrDefault(x => object.ReferenceEquals(x.Element, s));
        //            if (v != null) {
        //                ns.Add(v);
        //                _references.Remove(v);
        //            } else { ns.Add(_toSyncSet(s)); }
        //        }
        //        var trash = _references;
        //        _references = ns;
        //        return trash;
        //    }
        //}
        //internal virtual IEnumerable<SynchroSet> ToConvertedFilter(IEnumerable<SynchroSet> bfr) {
        //    return bfr;
        //}
        //void ReferenceToConverted(IEnumerable<SynchroSet> removes) {
        //    foreach (var rmv in removes) {
        //        if (rmv.SyncObj is TConv r) {
        //            _convedlist.Remove(r);
        //            _removeAction?.Invoke(r);
        //        }
        //    }
        //    var adds = ToConvertedFilter(_references).Select(x => x.SyncObj as TConv).ToList();
        //    var chash = adds.ToArray();
        //    foreach (var item in _convedlist) {
        //        if (adds.Any(a => object.ReferenceEquals(a, item))) {
        //            adds.Remove(item);
        //        }
        //    }


        //    //for (int i = 0; i < adds.Count; i++) {
        //    //    if (object.ReferenceEquals(_convedlist[i], adds[i])) { continue; } //nullは判別できないのでnull以外を揃える案
        //    //    else if (adds[i] != null && _convedlist.Contains(adds[i])) {//nullの場合、どちらにせよ判別できない？
        //    //        var idx = _convedlist.IndexOf(adds[i]);
        //    //        _convedlist.Move(idx, i);
        //    //    }
        //    //    else { 
        //    //        if(_convedlist.Count <= i) _convedlist.Add(adds[i]);
        //    //        else _convedlist.Insert(i, adds[i]);
        //    //    }
        //    //}
        //}
        //void OnCollecChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        //    var rmv = SourceToReference(e);
        //    this.ReferenceToConverted(rmv);
        //}



        void SetFilter(Expression<Func<TSrc,bool>> filter) {
            var memberExp = filter.Body as MemberExpression;
            if (memberExp == null) return;
            _currentFilter = memberExp.Member.Name;
            if (dicFtr.ContainsKey(_currentFilter)) {
                var getkey = filter.Compile();
                dicFtr[_currentFilter] = getkey;//new Func<TSync, object>(x => getkey(x));
            }
        }
        void setComparer(Expression<Func<TConv, object>> comparerKey, IComparer<object> comparer) {

        }
        void SetComparer<TKey>(Expression<Func<TSrc,TKey>> comparerKey, IComparer<TKey> comparer) {
            var memberExp = comparerKey.Body as MemberExpression;
            if (memberExp == null) return;
            _currentComparer = memberExp.Member.Name;
            if (dicCpr.ContainsKey(_currentFilter)) {
                var getkey = comparerKey.Compile();
                var compFunc = new Func<TSrc, object>(x => getkey(x));
                dicCpr[_currentComparer] = compFunc;
                //_comparer= new AnonymousComparer<TKey>((int)compFunc); ここを実装予定
            }
        }
        
    }
    
}
