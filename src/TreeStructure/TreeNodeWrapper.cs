using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>ツリー構造をなすノードをラップする<br/>参照は子孫方向へのみ広がります。</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">ラップされるノード</typeparam>
    /// <typeparam name="TWrpr">ラップするノード</typeparam>
    public abstract class TreeNodeWrapper<TSrc,TWrpr> : CompositeWrapper<TSrc,TWrpr>
        where TSrc : class, ITreeNode<TSrc>
        where TWrpr : TreeNodeWrapper<TSrc,TWrpr> {

        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected TreeNodeWrapper(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceNodeChildren => SourceNode?.Children;
    }
}
