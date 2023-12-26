using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {
    /// <summary>連動の停止とインスタンスの破棄が可能な、TreeNodeをラップするオブジェクト</summary>
    /// <remarks><inheritdoc/></remarks>
    /// <typeparam name="TSrc">ラップされるノード</typeparam>
    /// <typeparam name="TImtr">ラップするノード</typeparam>
    public abstract class TreeNodeImitator<TSrc, TImtr> : CompositeImitator<TSrc, TImtr>
    where TSrc : class, ITreeNode<TSrc>
    where TImtr : TreeNodeImitator<TSrc, TImtr> {
        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected TreeNodeImitator(TSrc sourceNode) : base(sourceNode) { }
        /// <summary><inheritdoc/></summary>
        protected override IEnumerable<TSrc>? SourceNodeChildren => SourceNode?.Children;
    }

}
