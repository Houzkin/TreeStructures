using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure {
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
        IReadOnlyList<TNode>? _readonlylist;
        /// <summary><inheritdoc/></summary>
        public override IReadOnlyList<TNode> Children =>_readonlylist ??= new ReadOnlyCollection<TNode>(ChildNodes);

        /// <summary><inheritdoc/></summary>
        protected override IList<TNode> ChildNodes { get; } = new List<TNode>();

        /// <summary>子ノードとして追加可能かどうかを示す</summary>
        public bool CanAddChild(TNode node) {
            return CanAddChildNode(node);
        }

        /// <summary><inheritdoc/></summary>
        /// <remarks>基底クラスではnullの追加は非許容です。</remarks>
        /// <param name="child">追加しようとする子ノード</param>
        /// <returns>null、Treeの循環、兄弟ノードとの重複をチェックします。</returns>
        protected override bool CanAddChildNode(TNode child) {
            if (child == null) return false;
            return base.CanAddChildNode(child);
        }
        /// <summary>子ノードとして追加する</summary>
        public TNode AddChild(TNode child) {
            if (CanAddChild(child)) AddChildProcess(child);
            return Self;
        }
        /// <summary>子ノードとして挿入する</summary>
        public TNode InsertChild(int index, TNode child) {
            if (CanAddChild(child)) InsertChildProcess(index, child);
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
            MoveChildProcess(oldIndex, newIndex);
            return Self;
        }

        #region Process
        /// <summary><inheritdoc/></summary>
        protected override void AddChildProcess(TNode child) {
            this.ThrowExceptionIfDisposed();
            base.AddChildProcess(child);
        }
        /// <summary><inheritdoc/></summary>
        protected override void InsertChildProcess(int index, TNode child) {
            this.ThrowExceptionIfDisposed();
            base.InsertChildProcess(index, child);
        }
        /// <summary><inheritdoc/></summary>
        protected override void MoveChildProcess(int oldIndex, int newIndex) {
            this.ThrowExceptionIfDisposed();
            base.MoveChildProcess(oldIndex, newIndex);
        }
        /// <summary><inheritdoc/></summary>
        protected override void RemoveChildProcess(TNode child) {
            this.ThrowExceptionIfDisposed();
            base.RemoveChildProcess(child);
        }
        /// <summary><inheritdoc/></summary>
        protected override void ClearChildProcess() {
            this.ThrowExceptionIfDisposed();
            base.ClearChildProcess();
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
            foreach (var cld in this.ChildNodes.OfType<IDisposable>().ToArray())
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
