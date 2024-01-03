using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>Wraps a node forming a tree structure<br/>References spread only in the descendant direction.</summary>
    /// <remarks>Intended for exposing a mutable tree structure as a read-only or restricted data structure.</remarks>
    /// <typeparam name="TSrc">Type of the node to be wrapped</typeparam>
    /// <typeparam name="TWrpr">Type of the wrapper node</typeparam>
    public abstract class TreeNodeWrapper<TSrc,TWrpr> : CompositeWrapper<TSrc,TWrpr>
        where TSrc : class, ITreeNode<TSrc>
        where TWrpr : TreeNodeWrapper<TSrc,TWrpr> {

        /// <summary>Initializes a new instance</summary>
        /// <param name="sourceNode">Type of the node to be wrapped</param>
        protected TreeNodeWrapper(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceChildren => Source?.Children;
    }
}
