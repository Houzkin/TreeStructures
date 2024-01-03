using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {

    /// <summary>Represents a class that signifies a tree node with read-only associated values.</summary>
    /// <typeparam name="TSrc">The type of the original tree node.</typeparam>
    /// <typeparam name="TVal">The type of the value associated with each tree node.</typeparam>
    public sealed class ReadOnlyValuedTreeNode<TSrc,TVal> : CompositeWrapper<TSrc, ReadOnlyValuedTreeNode<TSrc,TVal>> where TSrc : class {
        private readonly Func<TSrc, IEnumerable<TSrc>> _toChildren;
        private readonly Func<TSrc, TVal> _toValue;
        internal ReadOnlyValuedTreeNode(TSrc sourceNode, Func<TSrc, IEnumerable<TSrc>> getChildren, Func<TSrc, TVal> toValue) : base(sourceNode) {
            _toChildren = getChildren;
            _toValue = toValue;
            //this.Value = _toValue(this.SourceNode);
        }
        internal static ReadOnlyValuedTreeNode<TNode,TVal> Create<TNode>(ITreeNode<TNode> node,Func<TNode,TVal> toValue) where TNode:class,ITreeNode<TNode> {
            return new ReadOnlyValuedTreeNode<TNode, TVal>((node as TNode)!, x => x.Children, toValue);
        }
        /// <inheritdoc/>
        protected override ReadOnlyValuedTreeNode<TSrc, TVal> GenerateChild(TSrc sourceChildNode) {
            if (sourceChildNode is null) return null;
            return new ReadOnlyValuedTreeNode<TSrc, TVal>(sourceChildNode, _toChildren, _toValue);
        }
        /// <inheritdoc/>
        protected override IEnumerable<TSrc>? SourceChildren => _toChildren(this.Source);

        /// <summary>Gets the value associated with the tree node.</summary>
        public TVal Value => _toValue(this.Source);
    }
    

    ///// <summary>
    ///// 各ノードの同一性をキーによって比較するGeneral treeを表す。
    ///// </summary>
    //internal abstract class KeyedTreeNode<TKey,TNode>:GeneralTreeNode<TNode> where TNode : KeyedTreeNode<TKey,TNode> {
    //    public KeyedTreeNode(TKey key) { Key = key; }

    //    /// <summary>When initialized with this constructor, the Key is set to the default value. Please override the Key property.</summary>
    //    protected KeyedTreeNode():this(default) { }
    //    public virtual TKey Key { get; }
    //    protected override bool CanAddChildNode([AllowNull] TNode child) {
    //        if(!base.CanAddChildNode(child)) return false;
    //        if (this.Upstream().Any(x => EqualityComparer.Equals(x.Key, child.Key))) return false;
    //        if (this.ChildNodes.Any(x => EqualityComparer.Equals(x.Key, child.Key))) return false;
    //        return true;
    //    }
    //    private IEqualityComparer<TKey>? _comparer;
    //    private IEqualityComparer<TKey> EqualityComparer => _comparer ??= SetupKeyEqualityComparer();

    //    protected virtual IEqualityComparer<TKey> SetupKeyEqualityComparer() {
    //        return EqualityComparer<TKey>.Default;
    //    }
    //}
    ///// <summary>
    ///// 各ノードの同一性をキーによって比較するGeneral treeを表す。
    ///// </summary>
    ///// <remarks>EqualityComparerはデフォルトが使用されます。カスタマイズする場合は<see cref="KeyedTreeNode{TKey, TNode}"/>を継承して使用してください。</remarks>
    ///// <typeparam name="TKey"></typeparam>
    //internal sealed class KeyedTreeNode<TKey> : KeyedTreeNode<TKey, KeyedTreeNode<TKey>> {
    //    public KeyedTreeNode(TKey key): base(key) { }
    //    //public override TKey Key { get => base.Key; }
    //}
}
