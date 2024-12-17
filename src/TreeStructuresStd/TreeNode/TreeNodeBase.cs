using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using TreeStructures.Collections;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>Represents a node that forms a tree structure.</summary>
    /// <typeparam name="TNode">The common base type for each node.</typeparam>
    [Serializable]
    public abstract class TreeNodeBase<TNode> : ITreeNode<TNode>
        where TNode:TreeNodeBase<TNode>,ITreeNode<TNode>{
        /// <summary>Initializes an instance of the class.</summary>
        protected TreeNodeBase() { }

        /// <summary>Gets or sets the parent node. <inheritdoc/></summary>
        public TNode? Parent { get; private set; }

        [NonSerialized]
        IEnumerable<TNode>? _readonlycollection;
        /// <summary>Gets the read-only collection of child nodes. </summary>
        public IEnumerable<TNode> Children => _readonlycollection ??= SetupPublicChildCollection(ChildNodes);

        [NonSerialized]
        IEnumerable<TNode>? _childNodes;
        /// <summary>Manages child nodes, an internal property for processing.</summary>
        protected IEnumerable<TNode> ChildNodes => _childNodes ??= SetupInnerChildCollection();
        /// <summary>Specifies the internal processing collection for handling child nodes.</summary>
        protected abstract IEnumerable<TNode> SetupInnerChildCollection();
        /// <summary>Specifies the public child node collection for external use.</summary>
        /// <param name="innerCollection">The internal collection of child nodes.</param>
        protected virtual IEnumerable<TNode> SetupPublicChildCollection(IEnumerable<TNode> innerCollection) {
            return innerCollection.AsReadOnly();
        }

        internal TNode Self => (this as TNode)!;

        /// <summary>Called by the parent node during processes like addition or removal.</summary>
        /// <param name="newParent">The new parent node.</param>
        /// <returns>True if the parent node is successfully set; otherwise, false.</returns>
        bool SetParent(TNode? newParent) {
            if (this.Parent == newParent) return false;
            this.Parent = newParent;
            return true;
        }

        /// <summary>Indicates whether the specified node can be added as a child node in the tree structure.</summary>
        /// <remarks>The base class allows adding null as a child.</remarks>
        /// <param name="child">The child node to be added.</param>
        /// <returns>Checks for tree cycles and duplicates with sibling nodes.</returns>
        protected virtual bool CanAddChildNode(TNode child) {
            if (child == null) return true;
            if (this.Upstream().Any(x => object.ReferenceEquals(x,child))) return false;

            if (this.ChildNodes.Any(x => object.ReferenceEquals(x, child)) && object.ReferenceEquals(child?.Parent, Self)) {
                return false;
            } else if (this.ChildNodes.Any(x => object.ReferenceEquals(x, child)) ^ object.ReferenceEquals(child?.Parent, Self)) {
                if (object.ReferenceEquals(child?.Parent, Self)) {
                    Debug.Assert(false,
                        "The recursive structure may not be functioning correctly.",
                        $"The node {child}, specified in the {nameof(Children)} of the current node {Self}, is not included, but the specified node is already set with the current node as its parent node.");
                } else {
                    Debug.Assert(false,
                        "The recursive structure may not be functioning correctly.",
                        $"The node {child} specified in the {nameof(Children)} of the current node {Self} is already included, but the specified node is not already set with the current node as its parent node.");
                }
            }
            return true;
        }

        #region edit processes
        /// <summary>Executes the process of replacing a child node.</summary>
        /// <remarks>
        /// If <paramref name="action"/> is null, it casts to <see cref="IList{T}"/>, 
        /// then uses the indexer to replace the element.
        /// </remarks>
        /// <param name="index">The index of the node to be replaced.</param>
        /// <param name="child">The node to be replaced.</param>
        /// <param name="action">
        /// Specifies the collection operation. Default behavior:
        /// <code>(collection, index, node) => ((IList&lt;TNode&gt;)collection)[index] = node;</code>
        /// </param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void SetChildProcess(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            if (!CanAddChildNode(child)) return;
            var bfr = this.childCash();
            action ??= (collection, index, node) => ((IList<TNode>)collection)[index] = node;
            try {
                var p = child?.Parent;
                child?.SetParent(Self);
                p?.RemoveChildProcess(child);
                var rmv = ChildNodes.ElementAt(index);
                if (rmv?.Parent == Self) rmv.SetParent(null);
                action(ChildNodes, index, child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"The specified replacement operation cannot be performed on {nameof(ChildNodes)}. Please check and ensure the argument {nameof(action)} is correctly specified.", ex);
            }catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        /// <summary>Executes the process of adding a child node at the specified index.</summary>
        /// <remarks>
        /// If <paramref name="action"/> is null, it casts to <see cref="IList{T}"/> and performs the insert operation. 
        /// </remarks>
        /// <param name="index">The index at which to add the child node.</param>
        /// <param name="child">The child node to be added.</param>
        /// <param name="action">
        /// Specifies the collection operation. Default behavior:
        /// <code>(collection, idx, node) => ((IList&lt;TNode&gt;)collection).Insert(idx, node);</code>
        /// </param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void InsertChildProcess(int index, TNode child,Action<IEnumerable<TNode>,int,TNode>? action = null) {
            if (!CanAddChildNode(child)) return;
            action ??= (collection, idx, node) => ((IList<TNode>)collection).Insert(idx, node);
            
            var bfr = this.childCash();
            try {
                var p = child?.Parent;
                child?.SetParent(Self);
                p?.RemoveChildProcess(child);
                action(ChildNodes,index, child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"The specified addition operation cannot be performed on {nameof(ChildNodes)}. Please check and ensure the argument {nameof(action)} is correctly specified.", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>Executes the process of removing a child node.</summary>
        /// <remarks>
        /// If <paramref name="action"/> is null, it casts to <see cref="ICollection{T}"/> and performs the remove operation.
        /// </remarks>
        /// <param name="child">The child node to be removed.</param>
        /// <param name="action">
        /// Specifies the collection operation. Default behavior:
        /// <code>(collection, node) => ((ICollection&lt;TNode&gt;)collection).Remove(node);</code>
        /// </param>
        protected virtual void RemoveChildProcess(TNode child,Action<IEnumerable<TNode>,TNode>? action = null) {
            action ??= (collection, node) => ((ICollection<TNode>)collection).Remove(node);
            var bfr = this.childCash();
            try {
                if (child?.Parent == Self) child.SetParent(null);
                action(ChildNodes,child);
            }catch(InvalidCastException ex) {
                throw new NotSupportedException($"The specified removal operation cannot be performed on {nameof(ChildNodes)}. Please check and ensure the argument {nameof(action)} is correctly specified.", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>Executes the process of clearing child nodes.</summary>
        /// <remarks>
        /// If <paramref name="action"/> is null, it casts to <see cref="ICollection{T}"/> and performs the clear operation.
        /// </remarks>
        /// <param name="action">
        /// Specifies the collection operation. Default behavior:
        /// <code>(collection) => ((ICollection&lt;TNode&gt;)collection).Clear();</code>
        /// </param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void ClearChildProcess(Action<IEnumerable<TNode>>? action = null) {
            action ??= (collection) => ((ICollection<TNode>)collection).Clear();
            var bfr = this.childCash();
            try {
                foreach (var item in this.ChildNodes.OfType<TNode>()) item.SetParent(null);
                action(this.ChildNodes);
            } catch (InvalidCastException ex) {
                throw new NotSupportedException($"The specified clear operation cannot be performed on {nameof(ChildNodes)}. Please check and ensure the argument {nameof(action)} is correctly specified.", ex);
            } catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }

        /// <summary>Executes the process of moving or swapping child nodes within the collection.</summary>
        /// <remarks>
        /// If <paramref name="action"/> is null, it casts to <see cref="IList{T}"/> and performs the shift process.
        /// </remarks>
        /// <param name="oldIndex">The index of the element to be moved.</param>
        /// <param name="newIndex">The new index to move the element to.</param>
        /// <param name="action">
        /// Specifies the collection operation. Default behavior:
        /// <code>(collection, newidx, oldidx) => {
        ///     var nodes = (IList&lt;TNode&gt;)collection;
        ///     var tgt = nodes[oldidx];
        ///     nodes.RemoveAt(oldidx);
        ///     nodes.Insert(newidx, tgt);
        /// };</code>
        /// </param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void ShiftChildProcess(int oldIndex,int newIndex,Action<IEnumerable<TNode>,int,int>? action = null) {
            action ??= (collection, newidx, oldidx) => {
                var nodes = (IList<TNode>)collection;
                var tgt = nodes[oldidx];
                nodes.RemoveAt(oldidx);
                nodes.Insert(newidx, tgt);
            };
            var bfr = this.childCash();
            try {
                action(this.ChildNodes, oldIndex, newIndex);
            } catch (InvalidCastException ex) {
                throw new NotSupportedException($"The specified shift operation cannot be performed on {nameof(ChildNodes)}. Please check and ensure the argument {nameof(action)} is correctly specified.", ex);
            }catch (Exception) {
                ErrAdj(bfr);
                throw;
            }
        }
        private TNode[] childCash() {
            return this.ChildNodes.OfType<TNode>().ToArray();
        }
        private void ErrAdj(TNode[] bfr) {
            var aft = childCash();
            var rmdchilds = bfr.Except(aft);
            var addchilds = aft.Except(bfr);
            var immtchilds = aft.Intersect(bfr);
            try {//削除されたノード過去のキャッシュ
                foreach (var rmd in rmdchilds) {
                    //現在の親とSelfが同じであればnullを設定
                    if (rmd.Parent == Self) rmd.SetParent(null);
                }
                foreach (var ads in addchilds) {//追加されたノード
                    if (ads.Parent != null && ads.Parent != Self && ads.Parent.Children.Contains(ads)) {
                        RemoveChildProcess(ads);
                    } else {
                        ads.SetParent(Self);
                    }
                }
                foreach (var ads in immtchilds) {
                    //子ノードの親への参照が変わっていたときの処理
                    if (ads.Parent != null && ads.Parent != Self && ads.Parent.Children.Contains(ads)) {
                        ads.Parent.RemoveChildProcess(ads);
                    } else {
                        ads.SetParent(Self);
                    }
                }
            }catch (Exception ignore) { }
        }

        /// <summary>
        /// Executes the Dispose process.
        /// <para>In the base class, <see cref="RemoveChildProcess(TNode, Action{IEnumerable{TNode}, TNode}?)"/> is executed.</para>
        /// </summary>
        /// <remarks>
        /// In the base class, the current node is detached from the parent node, and the Dispose method is called sequentially from the descendants' leaf nodes.
        /// </remarks>
        protected virtual void DisposeProcess() {
            try {
                this.Parent?.RemoveChildProcess(Self);
            } catch { }
            foreach (var cld in this.Evolve(a => a.ChildNodes, (a, b, c) => c.Concat(b).Prepend(a)).Skip(1).Reverse().OfType<IDisposable>()) {
                cld.Dispose();
            }
            //foreach (var cld in this.Levelorder().Skip(1).Reverse().OfType<IDisposable>()) {
            //    cld.Dispose();
            //}
        }
        #endregion
    }
}
