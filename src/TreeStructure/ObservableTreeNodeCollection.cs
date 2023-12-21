using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.EventManagement;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>観測可能な多分木構造をなすノードを表す</summary>
    /// <typeparam name="TNode">各ノードの共通基底クラスとなる型</typeparam>
    [Serializable]
    public class ObservableTreeNodeCollection<TNode> : TreeNodeCollection<TNode>, IObservableTreeNode<TNode>, INotifyPropertyChanged
        where TNode : ObservableTreeNodeCollection<TNode> {
        /// <summary>
        /// 新規インスタンスを初期化する
        /// </summary>
        public ObservableTreeNodeCollection() { this.PropertyChangeProxy = new PropertyChangeProxy(this); }
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="collection">コレクション</param>
        public ObservableTreeNodeCollection(IEnumerable<TNode> collection) : this() {
            foreach (var item in collection) { this.AddChild(item); }
        }
        //ReadOnlyObservableCollection<TNode>? _readonlyobservablecollection;
        ///// <inheritdoc/>
        //public override ReadOnlyObservableCollection<TNode> Children => _readonlyobservablecollection ??= new ReadOnlyObservableCollection<TNode>(ChildNodes);
        ///// <inheritdoc/>
        //protected override ObservableCollection<TNode> ChildNodes { get; } = new ObservableCollection<TNode>();

        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupInnerChildCollection() => new ObservableCollection<TNode>();
        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupPublicChildCollection(IEnumerable<TNode> innerCollection) 
            => new ReadOnlyObservableCollection<TNode>((innerCollection as ObservableCollection<TNode>)!);
        

        ///// <inheritdoc/>
        //protected override Action<IEnumerable<TNode>, int, int> MoveAction => (collection, oldIdx, newIdx) =>
        //    ((ObservableCollection<TNode>)collection).Move(oldIdx, newIdx);


        IDisposable DeferParentChangedNotification() {
            return UniqueExcutor.LateEvalute(parentchangedeventkey, () => Parent);
        }
        readonly string parentchangedeventkey = "in Library : " + nameof(ObservableTreeNodeCollection<TNode>)+ "." + nameof(Parent);
        readonly string disposedeventkey = "in Library : " + nameof(ObservableTreeNodeCollection<TNode>) + "." + nameof(Disposed);

        StructureChangedEventExecutor<TNode>? _uniqueExcutor;
        private StructureChangedEventExecutor<TNode> UniqueExcutor {
            get {
                if(_uniqueExcutor == null) {
                    _uniqueExcutor = new StructureChangedEventExecutor<TNode>(Self);
                    _uniqueExcutor.Register(disposedeventkey, () => Disposed?.Invoke(Self,EventArgs.Empty));
                    _uniqueExcutor.Register(parentchangedeventkey, () => RaisePropertyChanged(nameof(Parent)));
                }
                return _uniqueExcutor;
            }
        }
        PropertyChangeProxy PropertyChangeProxy;
        /// <summary>プロパティ変更通知を発行する</summary>
        /// <param name="propName"></param>
        protected void RaisePropertyChanged([CallerMemberName]string? propName = null) {
            PropertyChangeProxy.Notify(propName);
        }
        /// <summary>値の変更と変更通知の発行を行う</summary>
        protected virtual bool SetProperty<T>(ref T strage, T value, [CallerMemberName] string? propertyName = null) {
            return PropertyChangeProxy.SetWithNotify(ref strage, value, propertyName);
        }
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged {
            add { PropertyChangeProxy.Changed += value; }
            remove { PropertyChangeProxy.Changed -= value; }
        }
        /// <summary>ツリー構造が変化したとき発生する</summary>
        public event EventHandler<StructureChangedEventArgs<TNode>>? StructureChanged;
        void IObservableTreeNode<TNode>.OnStructureChanged(StructureChangedEventArgs<TNode> e) {
            StructureChanged?.Invoke(this, e);
        }
        /// <summary>破棄されたとき発生する</summary>
        public event EventHandler? Disposed;
        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            IDisposable? dsp = null;
            if (disposing){
                if(Disposed != null) {
                    dsp = UniqueExcutor.ExecuteUnique(disposedeventkey);
                }
            }
            base.Dispose(disposing);
            dsp?.Dispose();
            Disposed = null;
        }
        /// <inheritdoc/>
        protected override void AddChildProcess(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            using (child.UniqueExcutor.LateEvaluateTree())
            using (child.DeferParentChangedNotification()) {
                base.AddChildProcess(child,action);
            }
        }
        /// <inheritdoc/>
        protected override void InsertChildProcess(int index, TNode child,Action<IEnumerable<TNode>,int,TNode>? action = null) {
            using (child.UniqueExcutor.LateEvaluateTree())
            using (child.DeferParentChangedNotification()) {
                base.InsertChildProcess(index, child,action);
            }
        }
        /// <inheritdoc/>
        protected override void SetChildProcess(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            var rmv = ChildNodes.ElementAt(index);
            using (child.UniqueExcutor.LateEvaluateTree())
            using(child.DeferParentChangedNotification())
            using(rmv?.UniqueExcutor.LateEvaluateTree())
            using (rmv?.DeferParentChangedNotification())
                base.SetChildProcess(index, child,action);
        }
        /// <inheritdoc/>
        protected override void RemoveChildProcess(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            using (child?.UniqueExcutor.LateEvaluateTree()) 
            using (child?.DeferParentChangedNotification()) {
                base.RemoveChildProcess(child,action);
            }
        }
        /// <inheritdoc/>
        protected override void ClearChildProcess(Action<IEnumerable<TNode>>? action = null) {
            using (ChildNodes.Select(a => a?.UniqueExcutor.LateEvaluateTree()).OfType<IDisposable>().ToLumpDisposables()) 
            using (ChildNodes.Select(a => a?.DeferParentChangedNotification()).OfType<IDisposable>().ToLumpDisposables()) {
                base.ClearChildProcess(action);
            }
        }
        /// <summary>コレクション内の要素の移動を実行するプロセス。</summary>
        /// <param name="oldIndex">移動対象となる要素のインデックス</param>
        /// <param name="newIndex">移動先のインデックス</param>
        /// <param name="action">コレクションの操作を指示。以下、基底クラスでの指示<br/>
        /// <code>(collection, idx1, idx2) =>
        ///     ((ObservableCollection&lt;<typeparamref name="TNode"/>&gt;)collection).Move(idx1, idx2);
        /// </code>
        /// </param>
        protected override void ShiftChildProcess(int oldIndex, int newIndex,Action<IEnumerable<TNode>,int,int>? action = null) {
            action ??= (collection, idx1, idx2) =>
                ((ObservableCollection<TNode>)collection).Move(idx1, idx2);
            
            using (ChildNodes.ElementAt(oldIndex)?.UniqueExcutor.LateEvaluateTree()){
                base.ShiftChildProcess(oldIndex, newIndex,action);
            }
        }

    }
}
