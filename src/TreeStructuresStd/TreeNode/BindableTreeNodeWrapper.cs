using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>An object that wraps a <see cref="ITreeNode{TNode}"/> and is designed for data binding.</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">Type of the wrapped node</typeparam>
    /// <typeparam name="TWrpr">Type of the wrapping node</typeparam>
    public abstract class BindableTreeNodeWrapper<TSrc, TWrpr> : BindableHierarchyWrapper<TSrc, TWrpr>
    where TSrc : class, ITreeNode<TSrc>
    where TWrpr : BindableTreeNodeWrapper<TSrc, TWrpr> {
        /// <summary>Initializes a new instance.</summary>
        /// <param name="sourceNode">The node to be wrapped</param>
        protected BindableTreeNodeWrapper(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceChildren => Source?.Children;
    }

}
