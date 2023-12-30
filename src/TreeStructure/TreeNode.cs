using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>Represents a mutable node forming a general tree structure.</summary>
    /// <typeparam name="TNode">The common base type for each node.</typeparam>
    [Serializable]
    public abstract class TreeNode<TNode> : TreeNodeBase<TNode>, IMutableTreeNode<TNode>, IDisposable where TNode : TreeNode<TNode> {
        /// <summary>Initializes a new instance.</summary>
        protected TreeNode() {
            //Children = new ReadOnlyCollection<TNode>(ChildNodes);
        }
        /// <summary>Initializes a new instance.</summary>
        /// <param name="nodes"></param>
        protected TreeNode(IEnumerable<TNode> nodes) : this() {
            foreach (var node in nodes) { this.AddChild(node); }
        }
        
        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupInnerChildCollection() => new List<TNode>();

        /// <summary>Indicates whether the node can be added as a child node.</summary>
        public bool CanAddChild(TNode node) {
            return CanAddChildNode(node);
        }

        /// <summary><inheritdoc/></summary>
        /// <remarks>In the base class, adding null is not allowed.</remarks>
        /// <param name="child">The child node to be added.</param>
        /// <returns>Checks for null, tree cycles, and duplicates with sibling nodes.</returns>
        protected override bool CanAddChildNode([AllowNull] TNode child) {
            if (child == null) return false;
            return base.CanAddChildNode(child);
        }
        /// <summary>Adds the node as a child node.</summary>
        /// <returns>The current node.</returns>
        public TNode AddChild(TNode child) {
            AddChildProcess(child);
            return Self;
        }
        /// <summary>Inserts the node as a child node at the specified index.</summary>
        /// <returns>The current node.</returns>
        public TNode InsertChild(int index, TNode child) {
            InsertChildProcess(index, child);
            return Self;
        }

        /// <summary>Removes the specified child node.</summary>
        /// <returns>The removed child node.</returns>
        public TNode RemoveChild(TNode child) {
            if (ChildNodes.Contains(child))
                RemoveChildProcess(child);
            return child;
        }

        /// <summary>Removes all child nodes.</summary>
        /// <returns>The removed child nodes.</returns>
        public IReadOnlyList<TNode> ClearChildren() {
            var removedChildren = this.ChildNodes.OfType<TNode>().ToArray();
            ClearChildProcess();
            return removedChildren.Except(this.ChildNodes.OfType<TNode>()).ToArray();
        }

        /// <summary>Moves the child node from the old index to the new index.</summary>
        /// <param name="oldIndex">The original index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <returns>The current node.</returns>
        public TNode MoveChild(int oldIndex, int newIndex) {
            ShiftChildProcess(oldIndex, newIndex);
            return Self;
        }

        /// <summary>Replaces the child node at the specified index with a new node.</summary>
        /// <returns>The removed child node.</returns>
        public TNode SetChild(int index, TNode child) {
            var removedChild = ChildNodes.ElementAt(index);
            SetChildProcess(index, child);
            return removedChild;
        }

        #region Process
        /// <summary>Child node addition process.</summary>
        /// <remarks><paramref name="action"/> = null, it casts to <see cref="ICollection{T}"/> and adds the node.</remarks>
        /// <param name="child">The child node to add.</param>
        /// <param name="action">Specifies the collection operation. Default behavior:
        /// <code>(collection, node) => ((ICollection&lt;<typeparamref name="TNode"/>&gt;)collection).Add(node);</code></param>
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
        /// <summary>Gets a value indicating whether the current instance has already been disposed.</summary>
        protected bool IsDisposed {
            get { return _isDisposed; }
            private set { _isDisposed = value; }
        }
        /// <summary>Disposes the instance. Along with the target node, descendant nodes are also disposed.</summary>
        public void Dispose() {
            if (IsDisposed || _isDisposing) return;
            _isDisposing = true;
            this.Dispose(true);
            GC.SuppressFinalize(this);
            _isDisposing = false;
            return;
        }
        /// <summary>Disposes of resources.</summary>
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
        /// <summary>Prevents operations on an instance that has already been disposed.</summary>
        protected void ThrowExceptionIfDisposed() {
            if (IsDisposed) throw new ObjectDisposedException(this.ToString(), "既に破棄されたインスタンスが操作されました。");
        }
        /// <summary>Disposes of all descendant nodes.</summary>
        /// <returns>The current node.</returns>
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
