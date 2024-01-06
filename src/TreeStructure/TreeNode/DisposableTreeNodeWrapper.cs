using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>An object that wraps a <see cref="ITreeNode{TNode}"/>,providing the ability to dispose of the instance.</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">Type of the wrapped node</typeparam>
    /// <typeparam name="TWrpr">Type of the wrapping node</typeparam>
    public abstract class DisposableTreeNodeWrapper<TSrc, TWrpr> : DisposableHierarchyWrapper<TSrc, TWrpr>
    where TSrc : class, ITreeNode<TSrc>
    where TWrpr : DisposableTreeNodeWrapper<TSrc, TWrpr> {
        /// <summary>Initializes a new instance.</summary>
        /// <param name="sourceNode">The node to be wrapped</param>
        protected DisposableTreeNodeWrapper(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceChildren => Source?.Children;
    }

}
