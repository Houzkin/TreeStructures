using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {
    public abstract class KeyedTreeNode<TKey,TNode>:GeneralTreeNode<TNode> where TNode : KeyedTreeNode<TKey,TNode> {
        public KeyedTreeNode(TKey key) { Key = key; }

        /// <summary>When initialized with this constructor, the Key is set to the default value. Please override the Key property.</summary>
        protected KeyedTreeNode():this(default) { }
        public virtual TKey Key { get; }
        protected override bool CanAddChildNode([AllowNull] TNode child) {
            if(!base.CanAddChildNode(child)) return false;
            if (this.Upstream().Any(x => EqualityComparer.Equals(x.Key, child.Key))) return false;
            if (this.ChildNodes.Any(x => EqualityComparer.Equals(x.Key, child.Key))) return false;
            return true;
        }
        private IEqualityComparer<TKey>? _comparer;
        private IEqualityComparer<TKey> EqualityComparer => _comparer ??= SetupKeyEqualityComparer();

        protected virtual IEqualityComparer<TKey> SetupKeyEqualityComparer() {
            return EqualityComparer<TKey>.Default;
        }
    }
    public sealed class KeyedTreeNode<TKey> : KeyedTreeNode<TKey, KeyedTreeNode<TKey>> {
        public KeyedTreeNode(TKey key): base(key) { }
        //public override TKey Key { get => base.Key; }
    }
}
