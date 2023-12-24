using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures {

    /// <summary>N分木を表す</summary>
    /// <typeparam name="TNode">共通となる型</typeparam>
    public abstract class NAryTreeNode<TNode> : TreeNodeBase<TNode> where TNode : NAryTreeNode<TNode> {
        /// <summary>子ノードを既定の配列で管理するノードを初期化する</summary>
        /// <param name="nary"></param>
        protected NAryTreeNode(int nary) :base() {
            if (this.ChildNodes is null) {
                array = new TNode[nary];
            } else {
                var tp = ChildNodes.GetType();
                if (!tp.IsArray && ChildNodes is ICollection<TNode> lst) {
                    for(int i= lst.Count; i < nary; i++) {
                        lst.Add(default);
                    }
                }
            }
        }
        TNode[]? array;
        /// <inheritdoc/>
        protected override IEnumerable<TNode> SetupInnerChildCollection() => array;


        /// <summary>指定したインデックスの要素が null だった場合、子ノードを追加する。</summary>
        /// <param name="index"></param>
        /// <param name="child"></param>
        /// <param name="action">このパラメータは無効です</param>
        protected override void InsertChildProcess(int index, TNode child, Action<IEnumerable<TNode>, int, TNode>? action = null) {
            if(ChildNodes.ElementAt(index) == null)
                base.SetChildProcess(index, child);
        }
        /// <summary><inheritdoc/></summary>
        /// <param name="child">削除する子ノード</param>
        /// <param name="action">このパラメータは無効です</param>
        protected override void RemoveChildProcess(TNode child, Action<IEnumerable<TNode>, TNode>? action = null) {
            var idx = ChildNodes.ToList().IndexOf(child);
            if(0<=idx) base.SetChildProcess(idx, null);
        }
        /// <summary><inheritdoc/></summary>
        /// <remarks><see cref="IList{T}"/>へキャストして、全ての要素をnullに置き換える</remarks>
        /// <param name="action">コレクションの操作を指示。以下、基底クラスでの処理<br/>
        /// <code>collection => {
        /// var lst = (IList&lt;<typeparamref name="TNode"/>&gt;)collection;
        /// for (int i = 0; i &lt; lst.Count; i++) 
        ///     lst[i] = null;
        ///};
        /// </code></param>
        protected override void ClearChildProcess(Action<IEnumerable<TNode>>? action = null) {
            action ??= collection => {
                var lst = (IList<TNode>)collection;
                for(int i =0; i< lst.Count; i++) lst[i] = null;
            };
            base.ClearChildProcess(action);
        }
        /// <summary>null の要素に子ノードを追加する</summary>
        /// <param name="child"></param>
        protected virtual void AddChildProcess(TNode child) {
            var idx = ChildNodes.ToList().IndexOf(null);
            if (0 <= idx) base.SetChildProcess(idx, child);
        }
        /// <summary>指定したインデックスの要素を入替える</summary>
        /// <param name="idxA"></param>
        /// <param name="idxB"></param>
        protected virtual void SwapChildProcess(int idxA, int idxB) {
            base.ShiftChildProcess(idxA, idxB,
                (collection, idx1, idx2) => {
                    var col = (IList<TNode>)collection;
                    var itmA = col[idx1];
                    col[idx1] = col[idx2];
                    col[idx2] = itmA;
                });
        }
        /// <summary>現在のノードに指定されたノードが子ノードとして追加可能かどうか示す</summary>
        public bool CanAddChild([AllowNull]TNode child) {
            if (!base.CanAddChildNode(child)) return false;
            if (!ChildNodes.Any(x=>x==null)) return false;
            return true;
        }
        /// <summary>子ノードを追加する</summary>
        /// <returns>現在のノード</returns>
        public TNode AddChild(TNode child) {
            if (!CanAddChildNode(child)) return Self;
            this.AddChildProcess(child);
            return Self;
        }
        /// <summary>インデックスを指定して子ノードを追加する</summary>
        /// <returns>現在のノード</returns>
        public TNode SetChild(int index, TNode child) {
            if (!this.CanAddChildNode(child)) return Self;
            SetChildProcess(index, child);
            return Self;
        }
        /// <summary>ノードを削除する</summary>
        /// <returns>削除されたノード</returns>
        public TNode RemoveChild(TNode child) {
            RemoveChildProcess(child);
            return child;
        }
        /// <summary>指定したインデックスの要素を入替える</summary>
        /// <param name="idxA"></param>
        /// <param name="idxB"></param>
        /// <returns>現在のノード</returns>
        public TNode SwapChild(int idxA, int idxB) {
            this.SwapChildProcess(idxA, idxB);
            return Self;
        }
    }
    
}
