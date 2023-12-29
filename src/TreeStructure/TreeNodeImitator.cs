using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>An object that wraps a <see cref="ITreeNode{TNode}"/> with the ability to pause/resume synchronization and dispose the instance.</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">Type of the wrapped node</typeparam>
    /// <typeparam name="TImtr">Type of the wrapping node</typeparam>
    public abstract class TreeNodeImitator<TSrc, TImtr> : CompositeImitator<TSrc, TImtr>
    where TSrc : class, ITreeNode<TSrc>
    where TImtr : TreeNodeImitator<TSrc, TImtr> {
        /// <summary>Initializes a new instance.</summary>
        /// <param name="sourceNode">The node to be wrapped</param>
        protected TreeNodeImitator(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceNodeChildren => SourceNode?.Children;
    }

}
