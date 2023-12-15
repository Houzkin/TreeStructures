using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructures.Tree {

    /// <summary>N分木を表す</summary>
    /// <typeparam name="T">共通となる型</typeparam>
    public abstract class NAryTreeNode<T> : TreeNodeBase<T> where T : NAryTreeNode<T> {
        /// <summary>コンストラクタ</summary>
        /// <param name="nary"></param>
        protected NAryTreeNode(int nary) :base() {
            for(int i = 0; i < nary; i++) {
                this.AddAction(this.ChildNodes, null);
            }
        }
        /// <inheritdoc/>
        protected override IList<T> ChildNodes { get; } = new List<T>();
        /// <summary>子ノードの入替処理を実行するプロセス</summary>
        protected virtual void SetChildProcess(int index, T value) {
            base.RemoveChildProcess(ChildNodes.ElementAt(index));
            base.InsertChildProcess(index, value);
        }
        /// <summary>子ノードの削除処理を実行するプロセス</summary>
        protected override void RemoveChildProcess(T child) {
            var idx = ChildNodes/*.ToList()*/.IndexOf(child);
            if(0<=idx) this.SetChildProcess(idx, null);
        }
        /// <summary>現在のノードに指定されたノードが子ノードとして追加可能かどうか示す</summary>
        public bool CanAddChild(T child) {
            if (!base.CanAddChildNode(child)) return false;
            if (!ChildNodes.Any(x=>x==null)) return false;
            return true;
        }
        /// <summary>子ノードを追加する</summary>
        /// <returns>現在のノード</returns>
        public T AddChild(T child) {
            if (!base.CanAddChildNode(child)) return Self;
            var idx = this.ChildNodes/*.ToList()*/.IndexOf(null);
            if(0<=idx) 
                SetChildProcess(idx, child);
            return Self;
        }
        /// <summary>ノードを削除する</summary>
        /// <returns>削除されたノード</returns>
        public T RemoveChild(T child) {
            RemoveChildProcess(child);
            return child;
        }
    }
    
}
