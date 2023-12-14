using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure {
    /// <summary>ツリー構造をなすノードをラップする</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">ラップされるノード</typeparam>
    /// <typeparam name="TOur">ラップするノード</typeparam>
    public abstract class TreeNodeImitator<TSrc, TOur> : CompositeImitator<TSrc, TOur>
    where TSrc : class, ITreeNode<TSrc>
    where TOur : TreeNodeImitator<TSrc, TOur> {
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected TreeNodeImitator(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceNodeChildren => SourceNode?.Children;
    }

}
