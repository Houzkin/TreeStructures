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
    public sealed class ReadOnlyValuedTreeNode<TSrc,TVal> : HierarchyWrapper<TSrc, ReadOnlyValuedTreeNode<TSrc,TVal>> where TSrc : class {
        private readonly Func<TSrc, IEnumerable<TSrc>> _toChildren;
        private readonly Func<TSrc, TVal> _toValue;
        internal ReadOnlyValuedTreeNode(TSrc sourceNode, Func<TSrc, IEnumerable<TSrc>> getChildren, Func<TSrc, TVal> toValue) : base(sourceNode) {
            _toChildren = getChildren;
            _toValue = toValue;
            this.Value = _toValue(this.Source);
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
        protected override IEnumerable<TSrc>? SourceChildren => _toChildren(this.Source);//?.AsReadOnly();

        /// <summary>Gets the value associated with the tree node.</summary>
        public TVal Value { get; private set; }
    }
    
}
