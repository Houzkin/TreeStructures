using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {

    /// <summary>Represents an N-ary tree.</summary>
    /// <typeparam name="TNode">The common type for nodes.</typeparam>
    [Serializable]
    public abstract class NAryTreeNode<TNode> : TreeNodeBase<TNode> ,IMutableTreeNode<TNode> where TNode : NAryTreeNode<TNode> {
        /// <summary>Initializes a node that manages child nodes with a default array.</summary>
        /// <param name="nary">The N-ary parameter.</param>
        protected NAryTreeNode(int nary) :base() {
            if (this.ChildNodes is null) {
                array = new TNode[nary];
            } else {
                var tp = ChildNodes.GetType();
                if (!tp.IsArray && ChildNodes is ICollection<TNode> lst) {
                    for(int i= lst.Count; i < nary; i++) {
                        lst.Add(default);
                    }
                }
            }
        }
        [NonSerialized]
        TNode[]? array;
        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupInnerChildCollection() => array;


        /// <summary>Adds a child node if the element at the specified index is null.</summary>
        /// <param name="index">The index at which to add the child node.</param>
        /// <param name="child">The child node to add.</param>
        /// <param name="action">This parameter is not applicable.</param>
        protected override void InsertChildProcess(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            if(ChildNodes.ElementAt(index) == null)
                base.SetChildProcess(index, child);
        }
        /// <summary><inheritdoc/></summary>
        /// <param name="child">The child node to remove.</param>
        /// <param name="action">This parameter is not applicable.</param>
        protected override void RemoveChildProcess(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            var idx = ChildNodes.ToList().IndexOf(child);
            if(0<=idx) base.SetChildProcess(idx, null);
        }
        /// <summary><inheritdoc/></summary>
        /// <remarks>Casts to <see cref="IList{T}"/> and replaces all elements with null.</remarks>
        /// <param name="action">Specifies the collection operation. In the base class, the following process is performed:
        /// <code>collection => {
        /// var lst = (IList&lt;<typeparamref name="TNode"/>&gt;)collection;
        /// for (int i = 0; i &lt; lst.Count; i++) 
        ///     lst[i] = null;
        ///};
        /// </code></param>
        protected override void ClearChildProcess(Action<IEnumerable<TNode>>? action = null) {
            action ??= collection => {
                var lst = (IList<TNode>)collection;
                for(int i =0; i< lst.Count; i++) lst[i] = null;
            };
            base.ClearChildProcess(action);
        }
        /// <summary>Adds a child node to null elements.</summary>
        /// <param name="child">Child node to be added.</param>
        protected virtual void AddChildProcess(TNode child) {
            var idx = ChildNodes.ToList().IndexOf(null);
            if (0 <= idx) base.SetChildProcess(idx, child);
        }
        /// <summary>Swaps the elements at the specified indexes.</summary>
        /// <param name="idxA">Index of the first element.</param>
        /// <param name="idxB">Index of the second element.</param>
        protected virtual void SwapChildProcess(int idxA, int idxB) {
            base.ShiftChildProcess(idxA, idxB,
                (collection, idx1, idx2) => {
                    var col = (IList<TNode>)collection;
                    var itmA = col[idx1];
                    col[idx1] = col[idx2];
                    col[idx2] = itmA;
                });
        }
        /// <summary>Indicates whether the specified node can be added as a child node to the current node.</summary>
        public bool CanAddChild(TNode child) {
            if (!base.CanAddChildNode(child)) return false;
            if (!ChildNodes.Any(x=>x==null)) return false;
            return true;
        }
        /// <summary>Adds a child node.</summary>
        /// <returns>The current node.</returns>
        public TNode AddChild(TNode child) {
            if (!CanAddChildNode(child)) return Self;
            this.AddChildProcess(child);
            return Self;
        }

        /// <summary>Adds a child node at the specified index.</summary>
        /// <returns>The current node.</returns>
        public TNode SetChild(int index, TNode child) {
            if (!this.CanAddChildNode(child)) return Self;
            SetChildProcess(index, child);
            return Self;
        }

        /// <summary>Removes a node.</summary>
        /// <returns>The removed node.</returns>
        public TNode RemoveChild(TNode child) {
            RemoveChildProcess(child);
            return child;
        }

        /// <summary>Swaps elements at the specified indices.</summary>
        /// <param name="idxA"></param>
        /// <param name="idxB"></param>
        /// <returns>The current node.</returns>
        public TNode SwapChild(int idxA, int idxB) {
            this.SwapChildProcess(idxA, idxB);
            return Self;
        }

        TNode IMutableTreeNode<TNode>.InsertChild(int index, TNode child) {
            this.InsertChildProcess(index, child);
            return Self;
        }
        /// <summary>Removes all child nodes.</summary>
        /// <returns>The removed child nodes.</returns>
        public IReadOnlyList<TNode> ClearChildren() {
            var lst = this.Children.OfType<TNode>().ToArray();
            this.ClearChildProcess();
            return lst;
        }
    }
    
}
