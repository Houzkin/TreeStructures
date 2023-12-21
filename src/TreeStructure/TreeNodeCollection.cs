using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>多分木構造をなすノードを表す</summary>
    /// <typeparam name="TNode">各ノードの共通基底クラスとなる型</typeparam>
    [Serializable]
    public abstract class TreeNodeCollection<TNode> : TreeNodeBase<TNode>, ITreeNodeCollection<TNode>, IDisposable where TNode : TreeNodeCollection<TNode> {
        /// <summary>新規インスタンスを初期化する。</summary>
        protected TreeNodeCollection() {
            //Children = new ReadOnlyCollection<TNode>(ChildNodes);
        }
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="nodes"></param>
        protected TreeNodeCollection(IEnumerable<TNode> nodes) : this() {
            foreach (var node in nodes) { this.AddChild(node); }
        }
        //IReadOnlyList<TNode>? _readonlylist;
        ///// <summary><inheritdoc/></summary>
        //public new IReadOnlyList<TNode> Children => (base.Children as IReadOnlyList<TNode>)!;

        ///// <summary><inheritdoc/></summary>
        //protected override IEnumerable<TNode> ChildNodes { get; } = new List<TNode>();
        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupInnerChildCollection() => new List<TNode>();


        /// <summary>子ノードとして追加可能かどうかを示す</summary>
        public bool CanAddChild(TNode node) {
            return CanAddChildNode(node);
        }

        /// <summary><inheritdoc/></summary>
        /// <remarks>基底クラスではnullの追加は非許容です。</remarks>
        /// <param name="child">追加しようとする子ノード</param>
        /// <returns>null、Treeの循環、兄弟ノードとの重複をチェックします。</returns>
        protected override bool CanAddChildNode([AllowNull] TNode child) {
            if (child == null) return false;
            return base.CanAddChildNode(child);
        }
        /// <summary>子ノードとして追加する</summary>
        public TNode AddChild(TNode child) {
            AddChildProcess(child);
            return Self;
        }
        /// <summary>子ノードとして挿入する</summary>
        public TNode InsertChild(int index, TNode child) {
            InsertChildProcess(index, child);
            return Self;
        }
        /// <summary>子ノードから削除する</summary>
        public TNode RemoveChild(TNode child) {
            if (ChildNodes.Contains(child))
                RemoveChildProcess(child);
            return child;
        }
        /// <summary>子ノードを全て削除する</summary>
        /// <returns>削除された子ノード</returns>
        public IReadOnlyList<TNode> ClearChildren() {
            var clrs = this.ChildNodes.OfType<TNode>().ToArray();
            ClearChildProcess();
            return clrs.Except(this.ChildNodes.OfType<TNode>()).ToArray();
        }
        /// <summary>子ノードの位置を移動する</summary>
        /// <param name="oldIndex">移動元</param>
        /// <param name="newIndex">移動先</param>
        /// <returns>現在のノード</returns>
        public TNode MoveChild(int oldIndex,int newIndex) {
            ShiftChildProcess(oldIndex, newIndex);
            return Self;
        }
        /// <summary>子ノードを差し替える</summary>
        /// <returns>削除された子ノード</returns>
        public TNode SetChild(int index, TNode child) {
            var rmv = ChildNodes.ElementAt(index);
            SetChildProcess(index, child);
            return rmv;
        }

        #region Process
        /// <summary>子ノードの追加プロセス</summary>
        /// <remarks><paramref name="action"/> = null で、<see cref="ICollection{T}"/>へキャストして追加する</remarks>
        /// <param name="child"></param>
        /// <param name="action">コレクションの操作を指示。以下、デフォルトの処理<br/>
        /// <code>(collection, node)=>((ICollection&lt;<typeparamref name="TNode"/>&gt;)collection).Add(node);</code></param>
        protected virtual void AddChildProcess(TNode child,Action<IEnumerable<TNode>,TNode>? action = null) {
            this.ThrowExceptionIfDisposed();
            //base.AddChildProcess(child);
            action ??= (collection, node) => ((ICollection<TNode>)collection).Add(node);
            base.InsertChildProcess(0, child, (collection, idx, node) => action(collection,node));
        }
        /// <inheritdoc/>
        protected override void InsertChildProcess(int index, TNode child,Action<IEnumerable<TNode>,int,TNode>? action = null) {
            this.ThrowExceptionIfDisposed();
            base.InsertChildProcess(index, child,action);
        }
        /// <inheritdoc/>
        protected override void SetChildProcess(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            this.ThrowExceptionIfDisposed();
            base.SetChildProcess(index, child,action);
        }
        /// <inheritdoc/>
        protected override void ShiftChildProcess(int oldIndex, int newIndex, Action<IEnumerable<TNode>, int, int>? action = null) {
            this.ThrowExceptionIfDisposed();
            base.ShiftChildProcess(oldIndex, newIndex, action);
        }
        /// <inheritdoc/>
        protected override void RemoveChildProcess(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            this.ThrowExceptionIfDisposed();
            base.RemoveChildProcess(child, action);
        }
        /// <inheritdoc/>
        protected override void ClearChildProcess(Action<IEnumerable<TNode>>? action = null) {
            this.ThrowExceptionIfDisposed();
            base.ClearChildProcess(action);
        }

        #endregion process

        #region Dispose
        [NonSerialized]
        bool _isDisposed;
        [NonSerialized]
        bool _isDisposing;
        /// <summary>現在のインスタンスが既に破棄されているかどうかを示す値を取得する。</summary>
        protected bool IsDisposed {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }
        /// <summary>インスタンスを破棄する。対象ノードに加え、子孫ノードも破棄される。</summary>
        public void Dispose() {
            if (IsDisposed || _isDisposing) return;
            _isDisposing = true;
            this.Dispose(true);
            GC.SuppressFinalize(this);
            _isDisposing = false;
            return;
        }
        /// <summary>リソースを破棄する。</summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.DisposeProcess();
            }
            IsDisposed = true;
        }
        void IDisposable.Dispose() {
            this.Dispose();
        }
        /// <summary>既に破棄されたインスタンスの操作を禁止する。</summary>
        protected void ThrowExceptionIfDisposed() {
            if (IsDisposed) throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
        }
        /// <summary>子孫ノードを全て破棄する。</summary>
        /// <returns>現在のノード</returns>
        public TNode DisposeDescendants() {
            foreach (var cld in this.ChildNodes.OfType<IDisposable>().Reverse())
                cld.Dispose();
            return Self;
        }
        ///// <summary>ファイナライズ</summary>
        //~TreeNodeCollection() {
        //    this.Dispose(false);
        //}
        #endregion
        
    }
}
