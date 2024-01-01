using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>Represents a binary tree.</summary>
    /// <typeparam name="TNode">The common base type for each node.</typeparam>
    public abstract class BinaryTreeNode<TNode> : NAryTreeNode<TNode> where TNode : BinaryTreeNode<TNode> {
        /// <summary>Constructor.</summary>
        protected BinaryTreeNode() : base(2) { }
        /// <summary>The node indicated by index 0.</summary>
        public TNode? Left {
            get { return ChildNodes.ElementAt(0); }
            set { if (base.CanAddChildNode(value)) SetChildProcess(0, value); }
        }
        /// <summary>The node indicated by index 1.</summary>
        public TNode? Right {
            get { return ChildNodes.ElementAt(1); }
            set { if (base.CanAddChildNode(value)) SetChildProcess(1, value); }
        }
    }
}
