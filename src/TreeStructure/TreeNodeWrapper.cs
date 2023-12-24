using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>ツリー構造をなすノードをラップする<br/>参照は子孫方向へのみ広がります。</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">ラップされるノード</typeparam>
    /// <typeparam name="TOur">ラップするノード</typeparam>
    public abstract class TreeNodeWrapper<TSrc,TOur> : CompositeWrapper<TSrc,TOur>
        where TSrc : class, ITreeNode<TSrc>
        where TOur : TreeNodeWrapper<TSrc,TOur> {

        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected TreeNodeWrapper(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceNodeChildren => SourceNode?.Children;
    }
}
