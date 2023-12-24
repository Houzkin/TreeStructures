using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeStructures.Linq;

namespace TreeStructures {
    /// <summary>連動の停止とインスタンスの破棄が可能な、Compositeパターンをラップするオブジェクト</summary>
    /// <remarks>MVVMパターンでのViewModelとしての使用を想定しています</remarks>
    /// <typeparam name="TSrc">Compositeパターンをなす型</typeparam>
    /// <typeparam name="TOur">ラップするノードの型</typeparam>
    public abstract class CompositeImitator<TSrc, TOur> : CompositeWrapper<TSrc, TOur>
        where TSrc : class
        where TOur : CompositeImitator<TSrc, TOur> {

        /// <summary>新規インスタンスを初期化する</summary>
        /// <param name="sourceNode">ラップされるノード</param>
        protected CompositeImitator(TSrc sourceNode) : base(sourceNode) { }
        private protected override TOur GenerateAndSetupChild(TSrc sourceChildNode) {
            var cld = base.GenerateAndSetupChild(sourceChildNode);
            if (cld != null) {
                cld.Parent = this as TOur;
                cld.ImitateSourceSubTree();
            }
            return cld;
        }
        /// <summary>基底クラスでは<see cref="Dispose()"/>メソッドが呼び出される</summary>
        /// <remarks>インスタンスを再利用する場合は<see cref="StopImitate"/>メソッドを指定してください</remarks>
        /// <param name="removedNode"></param>
        protected override void ManageRemovedChild(TOur removedNode) {
            removedNode.Dispose();
        }
        /// <summary>現在のノードと子孫ノードの分解、各ノードの<see cref="SourceNode"/> の子ノードコレクションに対する購読解除を実行する</summary>
        public void StopImitate() {
            this.Parent = null;
            foreach (var child in this.Levelorder().Skip(1).Reverse().ToArray())
                child.StopImitate();
            _children?.StopImitateAndClear();
        }
        /// <summary><see cref="SourceNode"/>の子ノードコレクションに対する購読を再開する</summary>
        public void ImitateSourceSubTree() {
            ThrowExceptionIfDisposed();
            _children?.Imitate();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                var nd = this.Levelorder().Skip(1).Reverse().ToArray();
                this.StopImitate();
                _children?.Dispose();
                foreach (var n in nd) n.Dispose();
                base.Dispose(disposing);
            }
        }
        /// <summary>インスタンスを破棄する</summary>
        public void Dispose() { (this as IDisposable)?.Dispose(); }
    }
}
