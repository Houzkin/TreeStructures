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
using TreeStructure.EventManagement;
using TreeStructure.Internals;
using TreeStructure.Linq;

namespace TreeStructure.Collections {
    /// <summary>連動するコレクションを提供する</summary>
    public abstract class ImitableCollection : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable {
        protected readonly IEnumerable<object> _source;
        private bool disposedValue;
        /// <summary>インスタンスを初期化する</summary>
        /// <param name="source">連動元のコレクション</param>
        protected ImitableCollection(IEnumerable<object> source) { _source = source; }
        /// <summary><inheritdoc/></summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary><inheritdoc/></summary>
        public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;
        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        /// <param name="e"></param>
        protected void RaisePropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }
        /// <summary>
        /// コレクション変更通知を発行する
        /// </summary>
        /// <param name="e"></param>
        protected void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e) {
            CollectionChanged?.Invoke(this, e);
        }
        #region static methods
        /// <summary>インスタンス初期化を補助する</summary>
        /// <typeparam name="TSrc">連動元のコレクション要素の型</typeparam>
        /// <typeparam name="TConv">連動するコレクション要素の型</typeparam>
        /// <param name="collection">連動元</param>
        /// <param name="converter"><typeparamref name="TSrc"/>から対応する<typeparamref name="TConv"/>へ変換する</param>
        /// <param name="removedAction">コレクションから削除された時の処理</param>
        /// <returns></returns>
        public static ImitableCollection<TSrc,TConv> Create<TSrc,TConv>(IEnumerable<TSrc> collection,Func<TSrc,TConv> converter,Action<TConv> removedAction) where TConv : class  where TSrc : class{
            Func<object, ConvertPair<TSrc, TConv>> generator = x => {
                var m = x as TSrc;
                return new ConvertPair<TSrc, TConv>(m, converter(m));
            };
            return new ImitableCollection<TSrc,TConv>(collection, generator, removedAction);
        }
        #endregion

        #region Dispose
        /// <summary>継承先でリソースの破棄を追加</summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // TODO: マネージド状態を破棄します (マネージド オブジェクト)
            }
            // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
            // TODO: 大きなフィールドを null に設定します
            disposedValue = true;
        }
        /// <summary>
        /// インスタンスを破棄する
        /// </summary>
        public void Dispose() {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            if (disposedValue) return;
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
            
        }
        /// <summary>既に破棄されたインスタンスの操作を禁止する</summary>
        /// <exception cref="ObjectDisposedException"></exception>
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
    /// <summary><inheritdoc/></summary>
    /// <typeparam name="TConv">連動するコレクションの要素</typeparam>
    public class ImitableCollection<TConv> : ImitableCollection,IReadOnlyList<TConv> where TConv : class {
        LumpedDisopsables Disposables = new LumpedDisopsables();
        internal IList<ConvertPair> _references = new List<ConvertPair>();
        internal readonly Func<object, ConvertPair> _toSyncSet;
        //IDisposable _listListener;
        internal Action<TConv> _removeAction;
        //readonly Func<object, IDisposable> _getSrcObjListener;
        //ObservableCollection<TConv> _convedlist = new();
        internal ImitableCollection(IEnumerable<object> collection,Func<object,ConvertPair> toSetConverter,Action<TConv> removedAction): base(collection) {

            _toSyncSet = toSetConverter;
            _removeAction = removedAction;
            SetReferenceWithStartObserveCollection();
        }
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="collection">連動元</param>
        /// <param name="converter"><see cref="object"/>から対応する<typeparamref name="TConv"/>へ変換する</param>
        /// <param name="removedAction">コレクションから削除された時の処理</param>
        public ImitableCollection(IEnumerable<object> collection,Func<object,TConv> converter,Action<TConv> removedAction) 
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
        /// <summary><inheritdoc/></summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing) {
            if(disposing) { this.StopImitateAndClear(); }
            base.Dispose(disposing);
        }
        /// <summary>現在連動状態かどうかを示す</summary>
        public bool IsImitating { get; private set; } = true;
        bool SwitchConnection(bool observe) {
            if (IsImitating == observe) return false;
            IsImitating = observe;
            return true;
        }
        /// <summary>連動状態でなかった場合、連動を開始する</summary>
        public void Imitate() {
            if (!SwitchConnection(true)) return;
            SetReferenceWithStartObserveCollection();
            var ary = _references.Select(x => x.After).ToArray();
            if (ary.Any()) {
                for(int i=0; i < ary.Length;i++) {
                    this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ary[i], i));
                }
            }
        }
        /// <summary>連動を停止する</summary>
        public void StopImitateAndClear() {
            if(!SwitchConnection(false)) return;
            this.Disposables.Dispose();
            _references.Clear();
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

    /// <summary><inheritdoc/></summary>
    public class ImitableCollection<TSrc,TConv> : ImitableCollection<TConv> where TSrc :class where TConv : class {
        internal ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, ConvertPair<TSrc, TConv>> toSetConverter, Action<TConv> removedAction)
            : base(collection: collection, toSetConverter: src => toSetConverter(src as TSrc), removedAction: removedAction) {


        }
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="collection">連動元</param>
        /// <param name="converter"><typeparamref name="TSrc"/>から対応する<typeparamref name="TConv"/>へ変換する</param>
        /// <param name="removedAction">コレクションから削除された時の処理</param>
        public ImitableCollection(IEnumerable<TSrc> collection, Func<TSrc, TConv> converter, Action<TConv> removedAction)
            : this(collection, new Func<TSrc, ConvertPair<TSrc, TConv>>(src => new ConvertPair<TSrc, TConv>(src, converter(src))), removedAction) { }
       
        
    }
    
}
