using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Tree {
    /// <summary>バイナリーツリーを表す</summary>
    /// <typeparam name="T">共通となる型</typeparam>
    public abstract class BinaryTreeNode<T> : NAryTreeNode<T> where T : BinaryTreeNode<T> {
        /// <summary>コンストラクタ</summary>
        protected BinaryTreeNode() : base(2) { }
        /// <summary>インデックス０で示すノード</summary>
        public T? Left {
            get { return ChildNodes.ElementAt(0); }
            set { if (base.CanAddChildNode(value)) SetChildProcess(0, value); }
        }
        /// <summary>インデックス1で示すノード</summary>
        public T? Right {
            get { return ChildNodes.ElementAt(1); }
            set { if (base.CanAddChildNode(value)) SetChildProcess(1, value); }
        }
    }
}
